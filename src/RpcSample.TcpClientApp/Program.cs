using System;
using System.Threading.Tasks;
using Lakerfield.RpcTest;

Console.WriteLine("Hello, World!");

var clientX = new Lakerfield.Rpc.NetworkClient("localhost", 3000);

//var pingResponse = await clientX.ExecutePing();

//pingResponse.

var client = new RpcTestServiceClient(clientX);

var company = await client.CompanyFindById(Guid.NewGuid());

Console.WriteLine(company.Name);

var subscription = client
  .GetObservable(Guid.NewGuid())
  .Subscribe(c => Console.WriteLine(c?.Name));

Console.ReadKey();

subscription.Dispose();
