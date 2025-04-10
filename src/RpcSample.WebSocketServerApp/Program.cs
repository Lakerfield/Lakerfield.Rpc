using Microsoft.AspNetCore.Builder;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Lakerfield.Rpc;
using Microsoft.Extensions.DependencyInjection;

namespace RpcSample.WebServerApp
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddRpcWebSocketServer<IRpcTestService, RpcTestWebSocketServiceServer>();

      var app = builder.Build();

      app.MapGet("/", () => "Hello World!");


      var webSocketOptions = new WebSocketOptions
      {
        KeepAliveInterval = TimeSpan.FromSeconds(120),
        AllowedOrigins = { "*" } // Pas dit aan voor productie!
      };

      app.UseWebSockets(webSocketOptions);

      app.UseRpcWebSocketServer<IRpcTestService>("/ws");

      //app.UseWebSockets();

      //app.Map("/ws", async context =>
      //{
      //  if (context.WebSockets.IsWebSocketRequest)
      //  {
      //    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
      //    await HandleWebSocketAsync(webSocket);
      //  }
      //  else
      //  {
      //    context.Response.StatusCode = 400;
      //  }
      //});

      await app.RunAsync();
    }

    // static async Task HandleWebSocketAsync(WebSocket webSocket)
    // {
    //   var buffer = new byte[1024];
    //   using var ms = new MemoryStream();
    //
    //   while (webSocket.State == WebSocketState.Open)
    //   {
    //     WebSocketReceiveResult result;
    //     do
    //     {
    //       result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    //       ms.Write(buffer, 0, result.Count);
    //     }
    //     while (!result.EndOfMessage); // Wachten tot het hele bericht binnen is
    //
    //     byte[] completeMessage = ms.ToArray();
    //     ms.SetLength(0); // Reset de stream voor het volgende bericht
    //
    //     if (result.MessageType == WebSocketMessageType.Binary)
    //     {
    //       Console.WriteLine($"Volledig binary bericht ontvangen: {BitConverter.ToString(completeMessage)}");
    //
    //       // Stuur het binaire bericht terug naar de client
    //       await webSocket.SendAsync(new ArraySegment<byte>(completeMessage), WebSocketMessageType.Binary, true, CancellationToken.None);
    //     }
    //   }
    // }

  }
}
