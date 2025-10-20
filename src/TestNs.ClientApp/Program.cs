using TestNs.Network;
using TestNs.Network.Models;

namespace TestNs.ClientApp
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Hello, World!");

      var networkClient = new Lakerfield.Rpc.NetworkClient(new Uri("ws://localhost:5000/rpc"));
      var client = new RpcTestNsClient(networkClient);

      await networkClient.Connected.ConfigureAwait(false);

      var x = await client.HelloHello();

      var o = client.Login(new LoginRequest()
      {
        Username = "Bertus",
        Password = "Test123!"
      });

      var sub = o.Subscribe(response =>
      {
        Console.WriteLine($"Token: {response.Name}");
      },
      error =>
      {
        Console.WriteLine($"Exception {error.Message}");
      },
      () =>
      {
        Console.WriteLine($"Completed");
      });

      Console.ReadLine();

      sub.Dispose();
    }
  }
}
