using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Lakerfield.Rpc;

namespace RpcSample;

[RpcServer]
public partial class RpcTestWebSocketServiceServer : Lakerfield.Rpc.LakerfieldRpcWebSocketServer<IRpcTestService>
{
    public override ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(LakerfieldRpcWebSocketServerConnection connection)
    {
      return new ClientConnectionMessageHandler(connection as LakerfieldRpcWebSocketServerConnection<IRpcTestService>);
    }




    public partial class ClientConnectionMessageHandler
    {
      public async Task<Models.Company> CompanyFindById(System.Guid id)
      {
        await Task.Delay(100);
        //this.Connection.TriggerClose();
        return new Models.Company()
        {
          Id = id.ToString(),
          Name = "The company",
          Remarks = "cool",
        };
      }

      public IObservable<RpcSample.Models.Company> GetObservable(System.Guid id)
      {
        return Observable.Interval(TimeSpan.FromSeconds(1)).Select(i => new Models.Company()
        {
          Id = "TEST",
          Name = $"Company number {i}",
          Remarks = "x"
        }).Take(10);
      }
    }
}
