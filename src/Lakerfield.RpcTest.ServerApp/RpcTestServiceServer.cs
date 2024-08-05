using System;
using Lakerfield.Rpc;

namespace Lakerfield.RpcTest;

[RpcServer]
public partial class RpcTestServiceServer : Lakerfield.Rpc.LakerfieldRpcServer<IRpcTestService>
{
    // public RpcTestServiceServer(IPAddress ipAddress, int port) : base(new Lakerfield.Rpc.LakerfieldRpcMessageRouterFactory(), ipAddress, port)
    // {
    // }

    public override ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(LakerfieldRpcServerConnection connection)
    {
      return new ClientConnectionMessageHandler(connection);
    }




    public partial class ClientConnectionMessageHandler
    {

      public ClientConnectionMessageHandler(LakerfieldRpcServerConnection<IRpcTestService> connection)
      {

      }



    }
}
