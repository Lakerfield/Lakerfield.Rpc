using System;
using TestNs.Network.Contract;

namespace TestNs.Network
{
  public partial class RpcTestNsClient
  {
    public Lakerfield.Rpc.INetworkClient Client { get; }
    public RpcTestNsClient(Lakerfield.Rpc.INetworkClient client)
    {
      RpcContractBsonConfigurator.Configure();
      Client = client;
    }

    public async System.Threading.Tasks.Task<bool> HelloHello()
    {
      var request = new RpcMessageHelloHelloRequest() {  };
      var response = await Client.Execute<RpcMessageHelloHelloResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public System.IObservable<TestNs.Network.Models.User> Login(TestNs.Network.Models.LoginRequest theRequest)
    {
      var request = new RpcMessageLoginRequest() { _TheRequest = theRequest };
      var result = Client.ExecuteObservable<TestNs.Network.Models.User>(request);
      return result;
    }


  }
}
