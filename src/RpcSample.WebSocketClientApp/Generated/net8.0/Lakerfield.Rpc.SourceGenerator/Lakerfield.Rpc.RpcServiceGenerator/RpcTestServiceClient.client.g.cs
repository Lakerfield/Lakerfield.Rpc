using System;
using RpcSample;

namespace RpcSample
{
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
      var request = new RpcMessageCompanyFindByIdRequest() { _Id = id };
      var response = await Client.Execute<RpcMessageCompanyFindByIdResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<RpcSample.Models.Company[]> CompanyFindAll()
    {
      var request = new RpcMessageCompanyFindAllRequest() {  };
      var response = await Client.Execute<RpcMessageCompanyFindAllResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<RpcSample.Models.Company> CompanySave(RpcSample.Models.Company entity)
    {
      var request = new RpcMessageCompanySaveRequest() { _Entity = entity };
      var response = await Client.Execute<RpcMessageCompanySaveResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<bool> CompanyDelete(RpcSample.Models.Company entity)
    {
      var request = new RpcMessageCompanyDeleteRequest() { _Entity = entity };
      var response = await Client.Execute<RpcMessageCompanyDeleteResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public async System.Threading.Tasks.Task<(RpcSample.Models.Company, string)> CompanyTest(RpcSample.Models.Company entity, RpcSample.Models.Company entity2, string wouter, int bert)
    {
      var request = new RpcMessageCompanyTestRequest() { _Entity = entity, _Entity2 = entity2, _Wouter = wouter, _Bert = bert };
      var response = await Client.Execute<RpcMessageCompanyTestResponse>(request).ConfigureAwait(false);
      return response.Result;
    }

    public System.IObservable<RpcSample.Models.Company> GetObservable(System.Guid id)
    {
      var request = new RpcMessageGetObservableRequest() { _Id = id };
      var result = Client.ExecuteObservable<RpcSample.Models.Company>(request);
      return result;
    }

    public async System.Threading.Tasks.Task MyVoidTest(RpcSample.Models.Company entity)
    {
      var request = new RpcMessageMyVoidTestRequest() { _Entity = entity };
      var response = await Client.Execute<RpcMessageMyVoidTestResponse>(request).ConfigureAwait(false);
      
    }


  }
}
