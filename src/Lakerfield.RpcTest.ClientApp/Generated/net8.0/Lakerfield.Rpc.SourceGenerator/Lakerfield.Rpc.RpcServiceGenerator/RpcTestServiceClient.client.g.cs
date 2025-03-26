using System;
using Lakerfield.RpcTest;

namespace Lakerfield.RpcTest
{
  // server False client True
  public partial class RpcTestServiceClient
  {
    public Lakerfield.Rpc.NetworkClient Client { get; }
    public RpcTestServiceClient(Lakerfield.Rpc.NetworkClient client)
    {
      RpcTestServiceBsonConfigurator.Configure();
      Client = client;
    }

    public async System.Threading.Tasks.Task<Lakerfield.RpcTest.Models.Company> CompanyFindById(System.Guid id)
    {
      var request = new CompanyFindByIdRequest() { Id = id };
      var response = await Client.Execute<CompanyFindByIdResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<Lakerfield.RpcTest.Models.Company[]> CompanyFindAll()
    {
      var request = new CompanyFindAllRequest() {  };
      var response = await Client.Execute<CompanyFindAllResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<Lakerfield.RpcTest.Models.Company> CompanySave(Lakerfield.RpcTest.Models.Company entity)
    {
      var request = new CompanySaveRequest() { Entity = entity };
      var response = await Client.Execute<CompanySaveResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<bool> CompanyDelete(Lakerfield.RpcTest.Models.Company entity)
    {
      var request = new CompanyDeleteRequest() { Entity = entity };
      var response = await Client.Execute<CompanyDeleteResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<(Lakerfield.RpcTest.Models.Company, string)> CompanyTest(Lakerfield.RpcTest.Models.Company entity, Lakerfield.RpcTest.Models.Company entity2, string wouter, int bert)
    {
      var request = new CompanyTestRequest() { Entity = entity, Entity2 = entity2, Wouter = wouter, Bert = bert };
      var response = await Client.Execute<CompanyTestResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public System.IObservable<Lakerfield.RpcTest.Models.Company> GetObservable(System.Guid id)
    {
      var request = new GetObservableRequest() { Id = id };
      var result = Client.ExecuteObservable<Lakerfield.RpcTest.Models.Company>(request);
      return result;
    }


  }
}
