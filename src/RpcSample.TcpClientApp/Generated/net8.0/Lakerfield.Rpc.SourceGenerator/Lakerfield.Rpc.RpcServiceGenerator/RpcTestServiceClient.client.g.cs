using System;
using RpcSample;

namespace RpcSample
{
  // server False client True
  public partial class RpcTestServiceClient
  {
    public Lakerfield.Rpc.INetworkClient Client { get; }
    public RpcTestServiceClient(Lakerfield.Rpc.INetworkClient client)
    {
      RpcTestServiceBsonConfigurator.Configure();
      Client = client;
    }

    public async System.Threading.Tasks.Task<RpcSample.Models.Company> CompanyFindById(System.Guid id)
    {
      var request = new CompanyFindByIdRequest() { Id = id };
      var response = await Client.Execute<CompanyFindByIdResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<RpcSample.Models.Company[]> CompanyFindAll()
    {
      var request = new CompanyFindAllRequest() {  };
      var response = await Client.Execute<CompanyFindAllResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<RpcSample.Models.Company> CompanySave(RpcSample.Models.Company entity)
    {
      var request = new CompanySaveRequest() { Entity = entity };
      var response = await Client.Execute<CompanySaveResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<bool> CompanyDelete(RpcSample.Models.Company entity)
    {
      var request = new CompanyDeleteRequest() { Entity = entity };
      var response = await Client.Execute<CompanyDeleteResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<(RpcSample.Models.Company, string)> CompanyTest(RpcSample.Models.Company entity, RpcSample.Models.Company entity2, string wouter, int bert)
    {
      var request = new CompanyTestRequest() { Entity = entity, Entity2 = entity2, Wouter = wouter, Bert = bert };
      var response = await Client.Execute<CompanyTestResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public System.IObservable<RpcSample.Models.Company> GetObservable(System.Guid id)
    {
      var request = new GetObservableRequest() { Id = id };
      var result = Client.ExecuteObservable<RpcSample.Models.Company>(request);
      return result;
    }


  }
}
