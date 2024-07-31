using Lakerfield.Rpc;

namespace Lakerfield.RpcTest;

[RpcServer]
public partial class RpcTestServiceClient : IRpcTestService
{
  public void Evelien2()
  {
    Client.Execute<CompanyFindByIdResponse>(new CompanyFindByIdRequest()
    {

    })
    CompanySaveRequest

  }

}
