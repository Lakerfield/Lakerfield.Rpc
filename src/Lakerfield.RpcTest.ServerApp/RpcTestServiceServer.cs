﻿using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Lakerfield.Rpc;

namespace Lakerfield.RpcTest;

[RpcServer]
public partial class RpcTestServiceServer : Lakerfield.Rpc.LakerfieldRpcServer<IRpcTestService>
{
    // public RpcTestServiceServer(IPAddress ipAddress, int port) : base(new Lakerfield.Rpc.LakerfieldRpcMessageRouterFactory(), ipAddress, port)
    // {
    // }

    public override ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(LakerfieldRpcServerConnection connection)
    {
      return new ClientConnectionMessageHandler(connection);
    }




    public partial class ClientConnectionMessageHandler
    {

      public ClientConnectionMessageHandler(LakerfieldRpcServerConnection<IRpcTestService> connection)
      {

      }

      public async Task<Models.Company> CompanyFindById(System.Guid id)
      {
        await Task.Delay(100);
        return new Models.Company()
        {
          Id = id.ToString(),
          Name = "The company",
          Remarks = "cool",
        };
      }

      public IObservable<Lakerfield.RpcTest.Models.Company> GetObservable(System.Guid id)
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
