using System.Threading.Tasks;
using Lakerfield.Rpc;
using Lakerfield.RpcTest.Models;

namespace Lakerfield.RpcTest;

[RpcClient]
public partial class RpcTestServiceClient : IRpcTestService
{
  public async Task<Company> Evelien2()
  {
    var result = await Client.Execute<CompanyFindByIdResponse>(new CompanyFindByIdRequest()
    {

    });
    return result.Result;
  }

}
