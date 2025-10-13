using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RpcSample.WebClientApp
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Hello, World!");

      await Task.Delay(1000);

      var networkClient = new Lakerfield.Rpc.NetworkClient(new Uri("ws://localhost:5005/ws"));

      var client = new RpcTestServiceClient(networkClient);

      var company = await client.CompanyFindById(Guid.NewGuid());

      Console.WriteLine(company.Name);

      var subscription = client
        .GetObservable(Guid.NewGuid())
        .Subscribe(c => Console.WriteLine(c?.Name));

      Console.ReadKey();

      subscription.Dispose();

      await client.MyVoidTest(company);
    }
  }
}
