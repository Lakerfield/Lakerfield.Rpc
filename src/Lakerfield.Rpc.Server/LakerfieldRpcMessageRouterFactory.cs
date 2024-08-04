namespace Lakerfield.Rpc;

public class LakerfieldRpcMessageRouterFactory<T>
{
  public ILakerfieldRpcMessageRouter CreateRouter(LakerfieldRpcServerConnection<T> connection)
  {
    return new LakerfieldRpcMessageRouter(null, connection);
  }
}
