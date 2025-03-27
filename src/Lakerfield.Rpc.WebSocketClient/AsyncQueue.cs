using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lakerfield.Rpc;

public class AsyncQueue<T>
{
  private readonly Channel<T> _channel;

  public AsyncQueue(int capacity = 0)
  {
    var options = new BoundedChannelOptions(capacity > 0 ? capacity : int.MaxValue)
    {
      FullMode = BoundedChannelFullMode.Wait
    };
    _channel = Channel.CreateBounded<T>(options);
  }

  public ValueTask EnqueueAsync(T item)
  {
    return _channel.Writer.WriteAsync(item);
  }

  public ValueTask<T> DequeueAsync()
  {
    return _channel.Reader.ReadAsync();
  }
}
