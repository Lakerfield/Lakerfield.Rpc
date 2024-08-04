using Lakerfield.Rpc;

namespace Lakerfield.RpcTest;

[RpcServer]
public partial class RpcTestServiceServer : Lakerfield.Rpc.LakerfieldRpcServer<IRpcTestService>
{
    // public RpcTestServiceServer(IPAddress ipAddress, int port) : base(new Lakerfield.Rpc.LakerfieldRpcMessageRouterFactory(), ipAddress, port)
    // {
    // }

    // public override ILakerfieldRpcMessageRouter CreateConnectionMessageRouter(LakerfieldRpcServerConnection connection)
    // {
    //   throw new System.NotImplementedException();
    // }
}
