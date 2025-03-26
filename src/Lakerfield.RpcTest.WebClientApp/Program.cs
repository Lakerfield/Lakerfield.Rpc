using System.Net.WebSockets;
using System.Text;

namespace Lakerfield.RpcTest.WebClientApp
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Hello, World!");


      using var ws = new ClientWebSocket();
      Uri serverUri = new Uri("ws://localhost:5005/ws");
      await ws.ConnectAsync(serverUri, CancellationToken.None);

      // Binary data verzenden (bijv. een byte array)
      byte[] binaryData = Encoding.UTF8.GetBytes("Hello WebSocket!");
      await ws.SendAsync(new ArraySegment<byte>(binaryData), WebSocketMessageType.Binary, true, CancellationToken.None);

      Console.WriteLine("Binary data verzonden.");

      // Ontvangen van data
      var buffer = new byte[1024];
      var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
      Console.WriteLine($"Ontvangen: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");

      await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);

    }
  }
}
