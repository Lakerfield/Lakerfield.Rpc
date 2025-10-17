using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lakerfield.Rpc.Helpers;

namespace Lakerfield.Rpc
{
  public class NetworkClient : INetworkClient, IDisposable
  {
    private DateTime _lastUsedAt = DateTime.MinValue;
    private readonly object _connectionLock = new object();
    private ulong _messagesSend = 0;
    private ulong _messagesRecieved = 0;
    private Task _connectedTask;
    private Task _runTask;
    private CancellationTokenSource _cancellationTokenSource;
    public Task Connected { get { return _connectedTask; } }

    public NetworkClient(Uri uri)
    {
      _cancellationTokenSource = new CancellationTokenSource();

      _runTask = Run(uri, _cancellationTokenSource.Token);
    }


    public async Task<RpcMessage> ExecutePing()
    {
      var message = new DrieNulSendMessage<RpcMessage>()
      {
        Opcode = MessageOpcode.PingRequest
      };

      var response = await Execute(message, new TimeSpan(0, 5, 5));
      return response.Message;
    }

    public async Task<T> Execute<T>(RpcMessage message) where T : RpcMessage
    {
      var request = new DrieNulSendMessage<RpcMessage>()
      {
        Opcode = MessageOpcode.MessageRequest
      };
      request.Message = message;

      var response = await Execute(request, new TimeSpan(0, 5, 0));

      var result = response.Message;

      // Check for serverside exception
      var exceptionMessage = result as RpcExceptionMessage;
      if (exceptionMessage != null)
      {
        var exception = new LakerfieldRpcServerException(exceptionMessage.Message);
        exception.Data.Add(LakerfieldRpcServerException.StacktraceServerKey, exceptionMessage.Stacktrace);
        throw exception;
      }

      var checkForNullResult = result as T;
      if (checkForNullResult == null)
        throw new InvalidOperationException("Unexpected null response received");
      return checkForNullResult;
    }

    private readonly Dictionary<int, NetworkObservable> _networkObservables = new Dictionary<int, NetworkObservable>();
    public IObservable<T> ExecuteObservable<T>(RpcMessage message)
    {
      var networkObservable = new NetworkObservable<T>(this, message);
      lock (_networkObservables)
        _networkObservables.Add(networkObservable.ObservableId, networkObservable);
      return networkObservable.Observable;
    }

    internal async Task ExecuteObservable(NetworkObservable networkObservable, RpcMessage message)
    {
      var request = new DrieNulSendMessage<RpcMessage>()
      {
        Opcode = MessageOpcode.ObservableRequest,
        Flags = MessageFlags.None,
        ObservableId = networkObservable.ObservableId,
        Message = message
      };

      await Execute(request, TimeSpan.FromSeconds(30));
    }

    internal async Task ExecuteObservableDispose(NetworkObservable networkObservable)
    {
      var request = new DrieNulSendMessage<RpcMessage>()
      {
        Opcode = MessageOpcode.ObservableDispose,
        Flags = MessageFlags.None,
        ObservableId = networkObservable.ObservableId
      };

      await Execute(request, TimeSpan.FromSeconds(30));
    }

    private readonly Dictionary<int, TaskCompletionSource<DrieNulReceiveMessage<RpcMessage>>> _pendingTasks
      = new Dictionary<int, TaskCompletionSource<DrieNulReceiveMessage<RpcMessage>>>();
    private async Task<DrieNulReceiveMessage<RpcMessage>> Execute(DrieNulSendMessage<RpcMessage> sendMessage, TimeSpan timeout)
    {
      TaskCompletionSource<DrieNulReceiveMessage<RpcMessage>> taskCompletionSource = null;
      var isBroadcast = sendMessage.Flags.HasFlag(MessageFlags.Broadcast);
      if (!isBroadcast)
      {
        taskCompletionSource = new TaskCompletionSource<DrieNulReceiveMessage<RpcMessage>>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_pendingTasks)
          _pendingTasks.Add(sendMessage.RequestId, taskCompletionSource);
      }
      try
      {
        await SendMessage(sendMessage);
        if (isBroadcast)
          return null;

        await taskCompletionSource.Task.AwaitWithTimeout(timeout).ConfigureAwait(false);
      }
      finally
      {
        if (!isBroadcast)
          lock (_pendingTasks)
            _pendingTasks.Remove(sendMessage.RequestId);
      }

      if (taskCompletionSource.Task.Status == TaskStatus.Faulted && taskCompletionSource.Task.Exception != null)
        throw taskCompletionSource.Task.Exception;
      if (taskCompletionSource.Task.Status != TaskStatus.RanToCompletion)
      {
        taskCompletionSource.TrySetCanceled();
        throw new TimeoutException();
      }

      var response = taskCompletionSource.Task.Result;
      return response;
    }


    internal async Task HandleMessage(DrieNulReceiveMessage<RpcMessage> message)
    {
      NetworkObservable networkObservable;
      switch (message.Opcode)
      {
        case MessageOpcode.PingRequest:
        case MessageOpcode.PingReply:
        case MessageOpcode.SystemMessage:
        case MessageOpcode.MessageRequest:
        case MessageOpcode.MessageResponse:
        case MessageOpcode.ObservableRequest:
        case MessageOpcode.ObservableDispose:
          TaskCompletionSource<DrieNulReceiveMessage<RpcMessage>> tcs;
          lock (_pendingTasks)
            if (_pendingTasks.TryGetValue(message.ResponseTo, out tcs))
            {
              tcs.TrySetResult(message);
              return;
            }
          break;

        case MessageOpcode.ObservableOnNext:
        case MessageOpcode.ObservableOnException:
        case MessageOpcode.ObservableOnComplete:
          networkObservable = GetNetworkObservable(message.ObservableId);
          if (networkObservable != null)
            await networkObservable.Queue(message);
          return;
      }

      Console.WriteLine("Unknown/unhandled message: {0}", message.Opcode);
    }

    private NetworkObservable? GetNetworkObservable(int observableId)
    {
      NetworkObservable result;
      _networkObservables.TryGetValue(observableId, out result);
      return result;
    }









    private WebSocket? _webSocket;
    private async Task Run(Uri uri, CancellationToken cancellationToken)
    {
      try
      {
        using var ws = new ClientWebSocket();
        _connectedTask = ws.ConnectAsync(uri, cancellationToken);
        await _connectedTask;

        _webSocket = ws;
        await ProcessAsync(ws, cancellationToken);
        _webSocket = null;
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }
    }

    private async Task ProcessAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
      var buffer = new byte[8192];
      var ms = new MemoryStream();
      try
      {
        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
          WebSocketReceiveResult result;
          do
          {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
              Console.WriteLine("Server requests close.");
              await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
              return;
            }

            ms.Write(buffer, 0, result.Count);
          } while (!result.EndOfMessage);

          if (result.MessageType == WebSocketMessageType.Binary)
          {
            ms.Position = 0;

            _lastUsedAt = DateTime.UtcNow;
            _messagesRecieved++;

            var request = new DrieNulReceiveMessage<RpcMessage>();
            request.ReadFrom(ms);

            await HandleMessage(request);

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
        Cleanup();
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
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client requesting close", cancellationToken);
            break;
        }
      }
    }



    internal async Task SendMessage(DrieNulSendMessage<RpcMessage> message)
    {
      using var stream = new MemoryStream();

      message.WriteTo(stream);
      stream.Position = 0;
      await SendMessage(stream, message.RequestId);
    }


    internal async Task SendMessage(MemoryStream memoryStream, int requestId)
    {
      var webSocket = _webSocket;
      if (webSocket == null || webSocket.State != WebSocketState.Open)
      { // TODO: duplicate???
        Console.WriteLine($"Cannot send message: WebSocket is {_webSocket.State}");
        return;
      }

      lock (_connectionLock)
      {
        _lastUsedAt = DateTime.UtcNow;
        //_requestId = requestId;
      }
      // TODO lock around write???
      try
      {
        await SendMemoryStreamToWebSocket(memoryStream, _webSocket, CancellationToken.None);
        _messagesSend++;
      }
      catch (WebSocketException wsex)
      {
        Console.WriteLine($"WebSocket send error: {wsex.Message}");
        //HandleException(wsex);
        //await HandleDisconnectAsync(cancellationToken);
      }
      catch (Exception ex)
      {
        //HandleException(ex);
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



    public void Cleanup()
    {
      lock (_pendingTasks)
      {
        foreach (var pendingTask in _pendingTasks)
          pendingTask.Value.TrySetCanceled();
        _pendingTasks.Clear();
      }

      lock (_networkObservables)
      {
        foreach (var networkObservable in _networkObservables)
          networkObservable.Value.Queue(new DrieNulReceiveMessage<RpcMessage>()
          {
            Opcode = MessageOpcode.ObservableOnException,
            Message = new RpcExceptionMessage()
            {
              Message = "NetworkObservable aborted due to WebSocket closing"
            }
          });
        _networkObservables.Clear();
      }
    }


    public void Dispose()
    {
      if (!_cancellationTokenSource.IsCancellationRequested)
        _cancellationTokenSource.Cancel();
    }


  }
}
