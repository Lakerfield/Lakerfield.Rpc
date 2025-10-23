using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Lakerfield.Rpc
{
  public abstract class LakerfieldRpcWebSocketServer<T>
  {
    private readonly List<LakerfieldRpcWebSocketServerConnection<T>> _connections = new List<LakerfieldRpcWebSocketServerConnection<T>>();


    public abstract ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(LakerfieldRpcWebSocketServerConnection connection);
    public abstract void InitBsonClassMaps();

    public LakerfieldRpcWebSocketServerConnection<T>[] Connections
    {
      get
      {
        lock (_connections)
          return _connections.ToArray();
      }
    }

    internal async Task WebSocketHandler(HttpContext context)
    {
      var webSocket = await context.WebSockets.AcceptWebSocketAsync();

      var connection = new LakerfieldRpcWebSocketServerConnection<T>(
          webSocket,
          CreateConnectionMessageRouter);

      lock (_connections)
        _connections.Add(connection);

      Console.WriteLine(@"Connection {0} opened", connection.ConnectionId);
      //Globals.Service.Log(LogLevel.Debug, @"Connection {0} opened", connection.ConnectionId)
      //  .Wait();

      await connection.ProcessAsync();

      // Cleanup
      Console.WriteLine(@"Connection {0} closed - {1} messages handled", connection.ConnectionId, connection.MessageCounter);
      lock (_connections)
        _connections.Remove(connection);
      //Globals.Service.Log(LogLevel.Debug, @"Connection {0} closed - {1} messages handled", connection.ConnectionId, connection.MessageCounter)
      //  .Wait();
    }
  }

  public static class LakerfieldRpcWebSocketServerExtensions
  {
    public static IServiceCollection AddRpcWebSocketServer<TService, TImplementation>(this IServiceCollection services) where TImplementation : LakerfieldRpcWebSocketServer<TService>, new()
    {
      services.AddSingleton<LakerfieldRpcWebSocketServer<TService>>(serviceProvider => serviceProvider.GetService<TImplementation>());

      return services;
    }

    public static IApplicationBuilder UseRpcWebSocketServer<TService>(this IApplicationBuilder app, string pathMatch)
    {
      app.Map(pathMatch, a =>
      {
        a.UseMiddleware<LakerfieldRpcWebSocketServerMiddleware<TService>>();
      });

      return app;
    }

  }

  public class LakerfieldRpcWebSocketServerMiddleware<T>
  {
    private readonly RequestDelegate _next;
    private readonly LakerfieldRpcWebSocketServer<T> _server;

    public LakerfieldRpcWebSocketServerMiddleware(RequestDelegate next, LakerfieldRpcWebSocketServer<T> server)
    {
      _next = next;
      _server = server;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      if (!context.WebSockets.IsWebSocketRequest)
      {
        context.Response.StatusCode = 400;
        return;
      }

      await _server.WebSocketHandler(context);
    }
  }
}
