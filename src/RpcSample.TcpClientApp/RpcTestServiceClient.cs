using System.Threading.Tasks;
using Lakerfield.Rpc;
using RpcSample.Models;

namespace RpcSample;

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
