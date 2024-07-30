using System.Threading.Tasks;

namespace Lakerfield.Rpc;

[RpcService]
public partial interface ISampleRpcService
{
    Task<string> GetHelloWorldAsync();
    
    
    //Task<>
    
}
