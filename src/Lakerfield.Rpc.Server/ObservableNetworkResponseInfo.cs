using System;
using System.Threading.Tasks;

namespace Lakerfield.Rpc
{
  public abstract class NetworkObservable : IDisposable
  {
    public abstract void Subscribe(int requestId, LakerfieldRpcServerConnection connection);
    public abstract void Dispose();
  }

  public class NetworkObservable<T> : NetworkObservable
  {
    private readonly IObservable<T> _observable;
    private int _observableId;
    private LakerfieldRpcServerConnection _connection;
    private IDisposable? _disposable;

    public NetworkObservable(IObservable<T> observable)
    {
      _observable = observable;
    }

    public override void Subscribe(int observableId, LakerfieldRpcServerConnection connection)
    {
      if (_disposable != null)
        throw new InvalidOperationException(@"Only one subscription supported");

      _observableId = observableId;
      _connection = connection;
      _disposable = _observable.Subscribe(OnNext, OnError, OnCompleted);
    }

    private void OnNext(T obj)
    {
      _connection.SendObservableOnNext(_observableId, obj);
    }

    private void OnError(Exception exception)
    {
      _connection.SendObservableOnError(_observableId, exception);
    }

    private void OnCompleted()
    {
      _connection.SendObservableOnComplete(_observableId);
    }

    public override void Dispose()
    {
      if (_disposable == null)
        return;

      _disposable.Dispose();
      _disposable = null;
    }

  }
}
