using System;
using System.Threading.Tasks;
using Lakerfield.RpcTest;

Console.WriteLine("Hello, World!");
await Task.Delay(1000);

var clientX = new Lakerfield.Rpc.NetworkClient("localhost", 3000);

//var pingResponse = await clientX.ExecutePing();

//pingResponse.

var client = new RpcTestServiceClient(clientX);

var company = await client.CompanyFindById(Guid.NewGuid());

Console.WriteLine(company.Name);

