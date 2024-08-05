using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lakerfield.Rpc.Helpers;

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

  public abstract class LakerfieldRpcServerConnection
  {
    internal abstract void SendObservableOnNext(int observableId, object value);

    internal abstract void SendObservableOnError(int observableId, Exception exception);

    internal abstract void SendObservableOnComplete(int observableId);

  }

  public class LakerfieldRpcServerConnection<T> : LakerfieldRpcServerConnection
  {
    private static int _lastConnectionId = 0;

    private readonly object _connectionLock = new object();
    private readonly int _connectionId;
    private DrieNulConnectionState _state;
    private TcpClient? _tcpClient;
    private readonly ILakerfieldRpcClientMessageHandler _clientMessageHandler;
    private Stream? _stream; // either a NetworkStream or an SslStream wrapping a NetworkStream
    private readonly DateTime _createdAt;
    private DateTime _lastUsedAt; // set every time the connection is Released
    private int _messageCounter;
    private int _requestId;
    private readonly Dictionary<int, NetworkObservable> _networkObservables = new Dictionary<int, NetworkObservable>();

    //public Model.Klant Klant { get; internal set; }

    internal LakerfieldRpcServerConnection(
      TcpClient tcpClient,
      Func<LakerfieldRpcServerConnection<T>, ILakerfieldRpcClientMessageHandler> createMessageRouter,
      LakerfieldRpcServer<T> listener)
    {
      _createdAt = DateTime.Now;
      _connectionId = Interlocked.Increment(ref _lastConnectionId);
      _state = DrieNulConnectionState.Initial;
      _tcpClient = tcpClient;
      _clientMessageHandler = createMessageRouter(this);

      Console.WriteLine(@"Connection {0} opened", _connectionId);
      Globals.Service.Log(LogLevel.Debug, @"Connection {0} opened", _connectionId)
        .Wait();
      var clientTask = Task.Run(() => Open());
      clientTask.ContinueWith(t => listener.Cleanup(this));
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

    internal async Task Open()
    {
      if (_state != DrieNulConnectionState.Initial)
        throw new InvalidOperationException("Open called more than once.");

      _tcpClient.NoDelay = true; // turn off Nagle
      _tcpClient.ReceiveBufferSize = ClientExportDefaults.TcpReceiveBufferSize;
      _tcpClient.SendBufferSize = ClientExportDefaults.TcpSendBufferSize;

      try
      {
        var stream = (Stream)_tcpClient.GetStream();
        #region SSL
        //if (_serverInstance.Settings.UseSsl)
        //{
        //  var checkCertificateRevocation = true;
        //  var clientCertificateCollection = (X509CertificateCollection)null;
        //  var clientCertificateSelectionCallback = (LocalCertificateSelectionCallback)null;
        //  var enabledSslProtocols = SslProtocols.Default;
        //  var serverCertificateValidationCallback = (RemoteCertificateValidationCallback)null;

        //  var sslSettings = _serverInstance.Settings.SslSettings;
        //  if (sslSettings != null)
        //  {
        //    checkCertificateRevocation = sslSettings.CheckCertificateRevocation;
        //    clientCertificateCollection = sslSettings.ClientCertificateCollection;
        //    clientCertificateSelectionCallback = sslSettings.ClientCertificateSelectionCallback;
        //    enabledSslProtocols = sslSettings.EnabledSslProtocols;
        //    serverCertificateValidationCallback = sslSettings.ServerCertificateValidationCallback;
        //  }

        //  if (serverCertificateValidationCallback == null && !_serverInstance.Settings.VerifySslCertificate)
        //  {
        //    serverCertificateValidationCallback = AcceptAnyCertificate;
        //  }

        //  var sslStream = new SslStream(stream, false, serverCertificateValidationCallback, clientCertificateSelectionCallback);
        //  try
        //  {
        //    var targetHost = _serverInstance.Address.Host;
        //    sslStream.AuthenticateAsClient(targetHost, clientCertificateCollection, enabledSslProtocols, checkCertificateRevocation);
        //  }
        //  catch
        //  {
        //    try { stream.Close(); }
        //    catch { } // ignore exceptions
        //    try { tcpClient.Close(); }
        //    catch { } // ignore exceptions
        //    throw;
        //  }
        //  stream = sslStream;
        //}
        #endregion
        _stream = stream;
        _state = DrieNulConnectionState.Open;

        //new Authenticator(this, _serverInstance.Settings.Credentials)
        //    .Authenticate();

        // Get a stream object for reading and writing
        stream = GetNetworkStream();

        //var readTimeout = (int)_serverInstance.Settings.SocketTimeout.TotalMilliseconds;
        //if (readTimeout != 0)
        //  networkStream.ReadTimeout = readTimeout;

        int bytesRead;
        var bytes = new Byte[256];
        var firstMessage = true;

        // Read 4 bytes (int32) for message length
        while ((bytesRead = await stream.ReadAsync(bytes, 0, 4)) != 0)
        {
          while (bytesRead < 4)
          {
            int x;
            if ((x = await stream.ReadAsync(bytes, bytesRead, 4 - bytesRead)) == 0)
              break;
            bytesRead += x;
          }

          if (firstMessage)
          {
            if (bytes[0] == 0x16) // SSL handshake record
              goto closeConnection;
            firstMessage = false;
          }

          var messageLength = ReadBsonInt32(bytes);
          _lastUsedAt = DateTime.UtcNow;

          using (var memoryStream = new MemoryStream(messageLength - 4))
          {
            await stream.CopyStreamToStreamAsync(memoryStream, messageLength - 4);
            memoryStream.Position = 0;

            var request = new DrieNulReceiveMessage<RpcMessage>();
            request.ReadFrom(memoryStream, messageLength);

            _ = Task.Run(() => HandleMessage(request));
          }
        }

        closeConnection: ;
      }
      catch (IOException ioException) when (ioException.InnerException is SocketException socketException &&
                                            socketException.SocketErrorCode switch
                                            {
                                              SocketError.ConnectionReset => true,
                                              _ => false
                                            })
      {
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        // TODO log...
        //HandleException(ex);
        //throw;
      }

      // Shutdown and end connection
      Close();
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

          if (_stream != null)
          {
            try { _stream.Close(); }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { } // ignore exceptions
            _stream = null;
          }

          if (_tcpClient != null)
          {
            if (_tcpClient.Connected)
            {
              // even though MSDN says TcpClient.Close doesn't close the underlying socket
              // it actually does (as proven by disassembling TcpClient and by experimentation)
              try { _tcpClient.Close(); }
              // ReSharper disable once EmptyGeneralCatchClause
              catch { } // ignore exceptions
            }
            _tcpClient = null;
          }

          _state = DrieNulConnectionState.Closed;
        }
      }
    }







    internal void SendMessage(Stream stream, int requestId)
    {
      if (_state == DrieNulConnectionState.Closed) { throw new InvalidOperationException("Connection is closed."); }
      lock (_connectionLock)
      {
        _lastUsedAt = DateTime.UtcNow;
        _requestId = requestId;

        try
        {
          var networkStream = GetNetworkStream();
          var writeTimeout = (int)ClientExportDefaults.SocketTimeout.TotalMilliseconds;
          if (writeTimeout != 0)
            networkStream.WriteTimeout = writeTimeout;
          stream.Position = 0;
          stream.CopyTo(networkStream);
          _messageCounter++;
        }
        catch (Exception ex)
        {
          HandleException(ex);
          throw;
        }
      }
    }

    internal void SendMessage(DrieNulSendMessage<RpcMessage> message)
    {
      using (var stream = new MemoryStream())
      {
        message.WriteTo(stream);
        SendMessage(stream, message.RequestId);
       }
    }

    // private methods
    private bool AcceptAnyCertificate(
        object sender,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors
    )
    {
      return true;
    }

    private Stream GetNetworkStream()
    {
      if (_state == DrieNulConnectionState.Initial)
        throw new InvalidOperationException("Connection isn't connected.");
      return _stream;
    }

    private void HandleException(Exception ex)
    {
      // TODO
      Close();
    }


    public int ReadBsonInt32(byte[] buffer)
    {
      return buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
    }


  }
}
