namespace Lakerfield.Rpc;

public class LakerfieldRpcMessageRouterFactory
{
  public ILakerfieldRpcMessageRouter CreateRouter(LakerfieldRpcServerConnection connection)
  {
    return new LakerfieldRpcMessageRouter(null, connection);
  }
}
