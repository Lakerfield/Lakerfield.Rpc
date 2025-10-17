using Lakerfield.Rpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TestNs.Network.Contract;
using TestNs.Network.Server;

namespace TestNs.WebApp
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Hello, World!");

      var builder = WebApplication.CreateBuilder(args);

      builder.Services
        .AddRpcWebSocketServer<IRpcContract, RpcTestNsServer>();

      var app = builder.Build();

      app.UseRouting();

      var webSocketOptions = new Microsoft.AspNetCore.Builder.WebSocketOptions
      {
        KeepAliveInterval = TimeSpan.FromSeconds(120),
        AllowedOrigins = { "*" } // Pas dit aan voor productie!
      };
      app.UseWebSockets(webSocketOptions);

      app.UseRpcWebSocketServer<IRpcContract>("/rpc");

      app.MapGet("/", async context =>
      {
        await context.Response.WriteAsync("Hello Server World!");
      });

      // execute the mapped endpoints here, otherwise it will be executed as the last middleware
      app.UseEndpoints(_ => { });

      await app.RunAsync();
    }
  }
}
