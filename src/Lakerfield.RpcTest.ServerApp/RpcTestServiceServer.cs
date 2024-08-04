using System;
using Lakerfield.Rpc;

namespace Lakerfield.RpcTest;

[RpcServer]
public partial class RpcTestServiceServer : Lakerfield.Rpc.LakerfieldRpcServer<IRpcTestService>
{
    // public RpcTestServiceServer(IPAddress ipAddress, int port) : base(new Lakerfield.Rpc.LakerfieldRpcMessageRouterFactory(), ipAddress, port)
    // {
    // }

    public override ILakerfieldRpcMessageRouter CreateConnectionMessageRouter(LakerfieldRpcServerConnection connection)
    {
      return new LakerfieldRpcMessageRouter(null, connection);
    }


    public partial class RpcTestServiceServerMessageRouter//ClientConnection
    {

      public RpcTestServiceServerMessageRouter(LakerfieldRpcServerConnection<IRpcTestService> connection)
      {

      }

      public System.Threading.Tasks.Task<Lakerfield.RpcTest.Models.Company> CompanyFindById(System.Guid id)
      {
        throw new NotImplementedException();
      }
    }
}
