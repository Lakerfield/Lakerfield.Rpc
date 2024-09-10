using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lakerfield.Rpc.Helpers;

namespace Lakerfield.Rpc
{
  public abstract class NetworkObservable
  {
    // private static fields
    private static int _lastObservableId = 0;

    public int ObservableId { get; private set; }

    protected NetworkObservable()
    {
      ObservableId = Interlocked.Increment(ref _lastObservableId);
    }

    public abstract ValueTask Queue(DrieNulReceiveMessage<RpcMessage> message);
  }

  internal class NetworkObservable<T> : NetworkObservable
  {
    private readonly NetworkClient _server;
    private readonly IObservable<T> _observable;
    private IObserver<T> _observer;
    private bool _disposed;
    private readonly AsyncQueue<DrieNulReceiveMessage<RpcMessage>> _queue;

    public IObservable<T> Observable { get { return _observable; } }

    public NetworkObservable(NetworkClient server, RpcMessage message)
    {
      _queue = new AsyncQueue<DrieNulReceiveMessage<RpcMessage>>();
      _server = server;
      _observable = System.Reactive.Linq.Observable.Create<T>(
        async (IObserver<T> observer) =>
        {
          _observer = observer;
          await server.ExecuteObservable(this, message);
          _ = Task.Run(ProcessQueue);
          return Disposable.Create(DoDispose);
        }).Publish().RefCount();
    }

    public override ValueTask Queue(DrieNulReceiveMessage<RpcMessage> message)
    {
      return _queue.EnqueueAsync(message);
    }

    private async Task ProcessQueue()
    {
      while (!_disposed)
      {
        var message = await _queue.DequeueAsync();
        if (_disposed)
          return;

        switch (message.Opcode)
        {
          case MessageOpcode.ObservableOnNext:
            var payload = message.Message as RpcObservableMessage;
            _observer.OnNext((T)(payload == null ? null : payload.Value));
            break;

          case MessageOpcode.ObservableOnException:
            var exPayload = message.Message as RpcExceptionMessage;
            _observer.OnError(exPayload == null ? null : new LakerfieldRpcConnectionException(exPayload.Message));
            return;

          case MessageOpcode.ObservableOnComplete:
            _observer.OnCompleted();
            return;
        }
      }
    }

    private void DoDispose()
    {
      _disposed = true;

      var networkObservable = this;
      AsyncRunSyncHelper.RunSync(() => _server.ExecuteObservableDispose(networkObservable));
      _queue.EnqueueAsync(new DrieNulReceiveMessage<RpcMessage>()
      {
        ObservableId = ObservableId,
        Opcode = MessageOpcode.ObservableOnComplete,
      });
    }
  }
}
