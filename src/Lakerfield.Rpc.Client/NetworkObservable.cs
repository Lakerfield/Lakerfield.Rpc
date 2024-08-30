using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
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

    public abstract void OnNext(object obj);
    public abstract void OnError(Exception exception);
    public abstract void OnComplete();

  }

  internal class NetworkObservable<T> : NetworkObservable
  {
    private readonly NetworkClient _server;
    private readonly IObservable<T> _observable;
    private IObserver<T> _observer;
    private bool _disposed;
    public IObservable<T> Observable { get { return _observable; } }

    public NetworkObservable(NetworkClient server, RpcMessage message)
    {
      _server = server;
      _observable = System.Reactive.Linq.Observable.Create<T>(
        async (IObserver<T> observer) =>
        {
          // Created synchronize observer, ivm parallel onnext calls
          // http://stackoverflow.com/questions/12270642/reactive-extension-onnext
          _observer = Observer.Synchronize(observer);
          await server.ExecuteObservable(this, message);
          return Disposable.Create(DoDispose);
        }).Publish().RefCount();
    }

    public override void OnNext(object obj)
    {
      if (_disposed)
        return;
      _observer.OnNext((T)obj);
    }

    public override void OnError(Exception exception)
    {
      if (_disposed)
        return;
      _observer.OnError(exception);
    }

    public override void OnComplete()
    {
      if (_disposed)
        return;
      _observer.OnCompleted();
    }

    private void DoDispose()
    {
      _disposed = true;

      var networkObservable = this;
      AsyncRunSyncHelper.RunSync(() => _server.ExecuteObservableDispose(networkObservable));
    }
  }
}
