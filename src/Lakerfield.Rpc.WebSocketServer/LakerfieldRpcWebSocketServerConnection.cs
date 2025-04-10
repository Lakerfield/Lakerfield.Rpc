using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lakerfield.Rpc.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lakerfield.Rpc
{
  /// <summary>
  /// Represents the state of a connection.
  /// </summary>
  public enum DrieNulConnectionState
  {
    /// <summary>
    /// The connection has not yet been initialized.
    /// </summary>
    Initial,
    /// <summary>
    /// The connection is open.
    /// </summary>
    Open,
    /// <summary>
    /// The connection is closed.
    /// </summary>
    Closed
  }

  public abstract class LakerfieldRpcWebSocketServerConnection
  {
    internal abstract void SendObservableOnNext(int observableId, object value);

    internal abstract void SendObservableOnError(int observableId, Exception exception);

    internal abstract void SendObservableOnComplete(int observableId);

    public abstract void TriggerClose();
  }

  public class LakerfieldRpcWebSocketServerConnection<T> : LakerfieldRpcWebSocketServerConnection
  {
    private static int _lastConnectionId = 0;

    private readonly object _connectionLock = new object();
    private readonly int _connectionId;
    private DrieNulConnectionState _state;
    private WebSocket? _webSocket;
    private readonly ILakerfieldRpcClientMessageHandler _clientMessageHandler;
    private readonly DateTime _createdAt;
    private DateTime _lastUsedAt; // set every time the connection is Released
    private int _messageCounter;
    private int _requestId;
    private readonly Dictionary<int, NetworkObservable> _networkObservables = new Dictionary<int, NetworkObservable>();

    //public Model.Klant Klant { get; internal set; }

    internal LakerfieldRpcWebSocketServerConnection(
      WebSocket webSocket,
      Func<LakerfieldRpcWebSocketServerConnection<T>, ILakerfieldRpcClientMessageHandler> createMessageRouter)
    {
      _createdAt = DateTime.Now;
      _connectionId = Interlocked.Increment(ref _lastConnectionId);
      _state = DrieNulConnectionState.Initial;
      _webSocket = webSocket;
      _clientMessageHandler = createMessageRouter(this);
    }

    /// <summary>
    /// Gets the connection id.
    /// </summary>
    public int ConnectionId
    {
      get { return _connectionId; }
    }

    /// <summary>
    /// Gets the DateTime that this connection was created at.
    /// </summary>
    public DateTime CreatedAt
    {
      get { return _createdAt; }
    }

    /// <summary>
    /// Gets the DateTime that this connection was last used at.
    /// </summary>
    public DateTime LastUsedAt
    {
      get { return _lastUsedAt; }
      internal set { _lastUsedAt = value; }
    }

    /// <summary>
    /// Gets a count of the number of messages that have been sent using this connection.
    /// </summary>
    public int MessageCounter
    {
      get { return _messageCounter; }
    }

    /// <summary>
    /// Gets the RequestId of the last message sent on this connection.
    /// </summary>
    public int RequestId
    {
      get { return _requestId; }
    }

    // internal methods
    internal bool IsExpired()
    {
      var now = DateTime.UtcNow;
      return now > _createdAt + ClientExportDefaults.MaxConnectionLifeTime
          || now > _lastUsedAt + ClientExportDefaults.MaxConnectionIdleTime;
    }

    public override void TriggerClose()
    {
      this._webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Hi its me", CancellationToken.None);
    }


    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
      _state = DrieNulConnectionState.Open;

      var buffer = new byte[8192];
      var ms = new MemoryStream();
      try
      {
        while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
          WebSocketReceiveResult result;
          do
          {
            result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
              Console.WriteLine("Client wil sluiten.");
              await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
              return;
            }

            ms.Write(buffer, 0, result.Count);

          } while (!result.EndOfMessage); // wacht tot laatste fragment

          // binary verwerken
          if (result.MessageType == WebSocketMessageType.Binary)
          {
            ms.Position = 0;

            _lastUsedAt = DateTime.UtcNow;

            //var messageLength = ReadBsonInt32(bytes);

            var request = new DrieNulReceiveMessage<RpcMessage>();
            request.ReadFrom(ms);//, messageLength);

            _ = Task.Run(() => HandleMessage(request));

            ms.SetLength(0);
          }
        }
      }
      catch (WebSocketException wsex)
      {
        Console.WriteLine($"WebSocketException: {wsex.Message}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Fout in ProcessAsync: {ex.Message}");
      }
      finally
      {
        Close();

        switch (_webSocket.State)
        {
          case WebSocketState.CloseSent:
          case WebSocketState.CloseReceived:
          case WebSocketState.Closed:
          case WebSocketState.Aborted:
            break;

          default:
          case WebSocketState.None:
          case WebSocketState.Open:
          case WebSocketState.Connecting:
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closing the connection", cancellationToken);
            break;
        }

        _webSocket.Dispose();
        Console.WriteLine("WebSocket gesloten en opgeruimd.");
      }
    }

    private async Task HandleMessage(DrieNulReceiveMessage<RpcMessage> request)
    {
      Exception occurredException = null;
      DrieNulSendMessage<RpcMessage> reply;
      //Console.WriteLine("Request {0} {1}", request.RequestId, request.Opcode);
      if (request.Opcode == MessageOpcode.PingRequest)
      {
        // Ping request > send reply
        reply = new DrieNulSendMessage<RpcMessage>()
        {
          Opcode = MessageOpcode.PingReply,
        };
      }
      else if (request.Opcode == MessageOpcode.MessageRequest)
      {
        reply = new DrieNulSendMessage<RpcMessage>()
        {
          Opcode = MessageOpcode.MessageResponse,
        };

        try
        {
          reply.Message = await _clientMessageHandler.HandleMessage(request.Message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          occurredException = ex;
          reply.Flags = MessageFlags.Exception;
          reply.Message = new RpcExceptionMessage()
          {
            Message = ex.Message,
            Stacktrace = ex.StackTrace
          };
        }
      }
      else if (request.Opcode == MessageOpcode.ObservableRequest)
      {
        reply = new DrieNulSendMessage<RpcMessage>()
        {
          Opcode = MessageOpcode.ObservableRequest,
        };
        try
        {
          var networkObservable = _clientMessageHandler.HandleObservable(request.Message);
          lock (_connectionLock)
            _networkObservables.Add(request.ObservableId, networkObservable);
          networkObservable.Subscribe(request.ObservableId, this);
          reply.ObservableId = request.ObservableId;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          occurredException = ex;
          reply = new DrieNulSendMessage<RpcMessage>
          {
            Opcode = MessageOpcode.ObservableOnException,
            Flags = MessageFlags.Exception,
            ObservableId = request.ObservableId,
            Message = new RpcExceptionMessage()
            {
              Message = ex.Message,
              Stacktrace = ex.StackTrace
            }
          };
        }
      }
      else if (request.Opcode == MessageOpcode.ObservableDispose)
      {
        try
        {
          reply = new DrieNulSendMessage<RpcMessage>()
          {
            Opcode = MessageOpcode.ObservableDispose
          };
          NetworkObservable networkObservable;
          lock (_connectionLock)
          {
            networkObservable = _networkObservables[request.ObservableId];
            _networkObservables.Remove(request.ObservableId);
          }
          networkObservable.Dispose();
          reply.ObservableId = request.ObservableId;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          occurredException = ex;
          reply = new DrieNulSendMessage<RpcMessage>
          {
            Opcode = MessageOpcode.ObservableOnException,
            Flags = MessageFlags.Exception,
            ObservableId = request.ObservableId,
            Message = new RpcExceptionMessage()
            {
              Message = ex.Message,
              Stacktrace = ex.StackTrace
            }
          };
        }
      }
      else
      {
        // Unknown message
        reply = new DrieNulSendMessage<RpcMessage>()
        {
          Flags = MessageFlags.UnknownMessage,
          Opcode = MessageOpcode.SystemMessage,
        };
      }

      if (occurredException != null)
        await Globals.Service.Log(LogLevel.Error, occurredException, "Fout in HandleMessage");

      reply.ResponseTo = request.RequestId;
      SendMessage(reply);
    }

    internal override void SendObservableOnNext(int observableId, object value)
    {
      var reply = new DrieNulSendMessage<RpcMessage>
      {
        Opcode = MessageOpcode.ObservableOnNext,
        Flags = MessageFlags.None,
        ObservableId = observableId,
        Message = new RpcObservableMessage()
          {
            Value = value
          }
      };
      SendMessage(reply);
    }

    internal override void SendObservableOnError(int observableId, Exception exception)
    {
      var reply = new DrieNulSendMessage<RpcMessage>
      {
        Opcode = MessageOpcode.ObservableOnException,
        Flags = MessageFlags.Exception,
        ObservableId = observableId,
        Message = new RpcExceptionMessage()
        {
          Message = exception.Message,
          Stacktrace = exception.StackTrace
        }
      };
      try
      {
        SendMessage(reply);
      }
      catch (IOException)
      { }
    }

    internal override void SendObservableOnComplete(int observableId)
    {
      var reply = new DrieNulSendMessage<RpcMessage>
      {
        Opcode = MessageOpcode.ObservableOnComplete,
        Flags = MessageFlags.None,
        ObservableId = observableId,
        Message = null
      };
      SendMessage(reply);
    }

    internal void Close()
    {
      lock (_connectionLock)
      {
        if (_state != DrieNulConnectionState.Closed)
        {
          foreach (var networkObservable in _networkObservables)
          {
            try { networkObservable.Value.Dispose(); }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { } // ignore exceptions
          }

          _state = DrieNulConnectionState.Closed;
        }
      }
    }


    internal async Task SendMessage(MemoryStream memoryStream, int requestId)
    {
      if (_state == DrieNulConnectionState.Closed) { throw new InvalidOperationException("Connection is closed."); }
      if (_webSocket.State != WebSocketState.Open)
      { // TODO: duplicate???
        Console.WriteLine("Kan niet verzenden: WebSocket is niet open.");
        return;
      }

      lock (_connectionLock)
      {
        _lastUsedAt = DateTime.UtcNow;
        _requestId = requestId;
      }
      // TODO lock around write???
      try
      {
        await SendMemoryStreamToWebSocket(memoryStream, _webSocket, CancellationToken.None);
        _messageCounter++;
      }
      catch (WebSocketException wsex)
      {
        Console.WriteLine($"WebSocket send error: {wsex.Message}");
        HandleException(wsex);
        //await HandleDisconnectAsync(cancellationToken);
      }
      catch (Exception ex)
      {
        HandleException(ex);
        throw;
      }
    }

    private static async Task SendMemoryStreamToWebSocket(MemoryStream memoryStream, WebSocket webSocket, CancellationToken cancellationToken)
    {
      memoryStream.Position = 0; // Ensure we're at the start
      const int chunkSize = 8192;
      byte[] buffer = new byte[chunkSize];

      int bytesRead;
      while ((bytesRead = await memoryStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
      {
        var segment = new ArraySegment<byte>(buffer, 0, bytesRead);
        bool isLastChunk = memoryStream.Position == memoryStream.Length;
        await webSocket.SendAsync(segment, WebSocketMessageType.Binary, isLastChunk, cancellationToken);
      }
    }

    internal async Task SendMessage(DrieNulSendMessage<RpcMessage> message)
    {
      using var stream = new MemoryStream();

      message.WriteTo(stream);
      stream.Position = 0;
      await SendMessage(stream, message.RequestId);
    }

    private void HandleException(Exception ex)
    {
      // TODO
      Close();
    }

  }
}
