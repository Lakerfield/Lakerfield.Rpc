using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lakerfield.Rpc;
using TestNs.Network.Contract;
using TestNs.Network.Models;
using LoginRequest = TestNs.Network.Models.LoginRequest;

namespace TestNs.Network.Server;

[RpcServer]
public partial class RpcTestNsServer : Lakerfield.Rpc.LakerfieldRpcWebSocketServer<IRpcContract>
{
  public override ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(LakerfieldRpcWebSocketServerConnection connection)
  {
    return new ClientConnectionMessageHandler(connection as LakerfieldRpcWebSocketServerConnection<IRpcContract>);
  }



  public partial class ClientConnectionMessageHandler : IRpcContract
  {

    public ClientConnectionMessageHandler()
    {
    }

    public async Task<bool> HelloHello()
    {
      return true;
    }

    public IObservable<User> Login(LoginRequest theRequest)
    {

      return null;
    }
  }
}
