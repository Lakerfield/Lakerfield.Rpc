using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
    private readonly TaskCompletionSource<string> _connectedTaskCompletionSource;
    public Task<string> Connected { get { return _connectedTaskCompletionSource.Task; } }

    public NetworkClient(string host, int port = 30701, bool ipv6 = false)
    {
      _connectedTaskCompletionSource = new TaskCompletionSource<string>();
      var addressFamily = ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
      var ipEndPoint = ToIpEndPoint(addressFamily, host, port);

      Open(ipEndPoint);
    }

    /// <summary>
    /// Returns the server address as an IPEndPoint (does a DNS lookup).
    /// </summary>
    /// <param name="addressFamily">The address family of the returned IPEndPoint.</param>
    /// <returns>The IPEndPoint of the server.</returns>
    public IPEndPoint ToIpEndPoint(AddressFamily addressFamily, string host, int port)
    {
      var ipAddresses = Dns.GetHostAddresses(host);
      if (ipAddresses != null && ipAddresses.Length != 0)
        foreach (var ipAddress in ipAddresses)
          if (ipAddress.AddressFamily == addressFamily)
            return new IPEndPoint(ipAddress, port);

      _connectedTaskCompletionSource.TrySetCanceled();
      var message = string.Format("Unable to resolve host name '{0}'.", host);
      throw new LakerfieldRpcConnectionException(message);
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
        taskCompletionSource = new TaskCompletionSource<DrieNulReceiveMessage<RpcMessage>>();
        lock (_pendingTasks)
          _pendingTasks.Add(sendMessage.RequestId, taskCompletionSource);
      }
      try
      {
        //var connection = GetConnection();
        //connection.
        SendMessage(sendMessage);
        if (isBroadcast)
          return null;

        // TODO: Uitzoeken ofdat dit niet anders kan, dit gebruikt thread om te wachten...
        await Task.Run(() => taskCompletionSource.Task.Wait(timeout));
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


    internal void HandleMessage(DrieNulReceiveMessage<RpcMessage> message)
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
          if (_pendingTasks.TryGetValue(message.ResponseTo, out tcs))
          {
            tcs.TrySetResult(message);
            return;
          }
          break;

        case MessageOpcode.ObservableOnNext:
          networkObservable = GetNetworkObservable(message.ObservableId);
          var payload = message.Message as RpcObservableMessage;
          if (networkObservable != null)
            networkObservable.OnNext(payload == null ? null : payload.Value);
          return;

        case MessageOpcode.ObservableOnComplete:
          networkObservable = GetNetworkObservable(message.ObservableId);
          var exPayload = message.Message as RpcExceptionMessage;
          if (networkObservable != null)
            networkObservable.OnError(exPayload == null ? null : new LakerfieldRpcConnectionException(exPayload.Message));
          return;

        case MessageOpcode.ObservableOnException:
          networkObservable = GetNetworkObservable(message.ObservableId);
          if (networkObservable != null)
            networkObservable.OnComplete();
          return;
      }

      Console.WriteLine("Unknown message: {0}", message.Opcode);
    }

    private NetworkObservable? GetNetworkObservable(int observableId)
    {
      NetworkObservable result;
      _networkObservables.TryGetValue(observableId, out result);
      return result;
    }









    private TcpClient? _tcpClient;
    private Stream? _stream;
    private Task _receiveMessagesTask;

    private void Open(IPEndPoint ipEndPoint)
    {
      var tcpClient = new TcpClient(ipEndPoint.AddressFamily);
      tcpClient.NoDelay = true; // turn off Nagle
      tcpClient.ReceiveBufferSize = ClientExportDefaults.TcpReceiveBufferSize;
      tcpClient.SendBufferSize = ClientExportDefaults.TcpSendBufferSize;
      tcpClient.Connect(ipEndPoint);

      var stream = (Stream)tcpClient.GetStream();

      _tcpClient = tcpClient;
      _stream = stream;

      _receiveMessagesTask = Task.Run(() => ReceiveMessagesLoop());
    }


    private async void ReceiveMessagesLoop()
    {
      try
      {
        var networkStream = _stream;// GetNetworkStream();
        var readTimeout = (int)ClientExportDefaults.SocketTimeout.TotalMilliseconds;
        if (readTimeout != 0)
          networkStream.ReadTimeout = readTimeout;

        int bytesRead;
        var bytes = new Byte[256];

        // Read 4 bytes (int32) for message length
        while ((bytesRead = await networkStream.ReadAsync(bytes, 0, 4)) != 0)
        {
          while (bytesRead < 4)
          {
            int x;
            if ((x = await networkStream.ReadAsync(bytes, bytesRead, 4 - bytesRead)) == 0)
              break;
            bytesRead += x;
          }

          var messageLength = ReadBsonInt32(bytes);
          _lastUsedAt = DateTime.UtcNow;

          var message = new DrieNulReceiveMessage<RpcMessage>();
          using (var memoryStream = new MemoryStream(messageLength - 4))
          {
            await networkStream.CopyStreamToStreamAsync(memoryStream, messageLength - 4);
            memoryStream.Position = 0;

            message.ReadFrom(memoryStream, messageLength);
          }
          _ = Task.Run(() => HandleMessage(message));
          _messagesRecieved++;
        }
      }
      catch (Exception ex)
      {
        _connectedTaskCompletionSource.TrySetException(ex);
        Dispose();
        //HandleException(ex);
        //throw;
      }
      finally
      {
        _connectedTaskCompletionSource.TrySetCanceled();
        Dispose();
        //Close();
      }

      int ReadBsonInt32(byte[] buffer)
      {
        return buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
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

    private void SendMessage(Stream stream, int requestId)
    {
      if (_tcpClient == null)
        throw new InvalidOperationException("NetworkClient already disposed");

      //if (_state == DrieNulConnectionState.Closed) { throw new InvalidOperationException("Connection is closed."); }
      lock (_connectionLock)
      {
        _lastUsedAt = DateTime.UtcNow;
        //_requestId = requestId;

        try
        {
          var networkStream = _stream;// GetNetworkStream();
          var writeTimeout = (int)ClientExportDefaults.SocketTimeout.TotalMilliseconds;
          if (writeTimeout != 0)
          {
            networkStream.WriteTimeout = writeTimeout;
          }
          stream.Position = 0;
          stream.CopyTo(networkStream);
          _messagesSend++;
        }
        catch (Exception ex)
        {
          _connectedTaskCompletionSource.TrySetException(ex);
          Dispose();
          //HandleException(ex);
          throw;
        }
      }
    }





    public void Dispose()
    {
      if (_tcpClient != null)
      {
        _tcpClient.Close();
        _tcpClient = null;
      }
    }












  }
}
