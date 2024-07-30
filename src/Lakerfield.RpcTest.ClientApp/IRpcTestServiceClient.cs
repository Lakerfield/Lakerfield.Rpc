using System.Threading.Tasks;
using Lakerfield.Rpc;

namespace Lakerfield.RpcTest;

[RpcService]
public interface IRpcTestServiceClient : IRpcTestService
{
  public Task<string> GetHelloWorldAsync2();

}
