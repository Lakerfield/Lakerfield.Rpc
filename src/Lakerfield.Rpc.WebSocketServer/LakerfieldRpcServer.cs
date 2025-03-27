using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lakerfield.Rpc
{
  public abstract class LakerfieldRpcServer<T>
  {
    private readonly TcpListener _tcpListener;
    private readonly List<LakerfieldRpcServerConnection<T>> _connections = new List<LakerfieldRpcServerConnection<T>>();


    public abstract ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(LakerfieldRpcServerConnection connection);
    public abstract void InitBsonClassMaps();

    public LakerfieldRpcServer(IPEndPoint endPoint)
    {
      _tcpListener = new TcpListener(endPoint.Address, endPoint.Port);
    }

    public LakerfieldRpcServerConnection<T>[] Connections
    {
      get
      {
        lock (_connections)
          return _connections.ToArray();
      }
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
      InitBsonClassMaps();
      _tcpListener.Start();

      await AcceptNewClientsLoopAsync(stoppingToken);
    }

    private async Task AcceptNewClientsLoopAsync(CancellationToken stoppingToken)
    {
      try
      {
        while (!stoppingToken.IsCancellationRequested)
        {
          Console.WriteLine("Waiting for a connection... ");
          var tcpClient = await _tcpListener.AcceptTcpClientAsync(stoppingToken);

          var connection = new LakerfieldRpcServerConnection<T>(
            tcpClient,
            CreateConnectionMessageRouter,
            this);
          lock (_connections)
            _connections.Add(connection);
        }
      }
      catch (SocketException ex)
      {
        Console.WriteLine(@"Listener loop {0}", ex.Message);
      }
      finally
      {
        _tcpListener.Stop();
      }
    }

    internal void Cleanup(LakerfieldRpcServerConnection<T> connection)
    {
      Console.WriteLine(@"Connection {0} closed - {1} messages handled", connection.ConnectionId, connection.MessageCounter);
      lock (_connections)
        _connections.Remove(connection);
      Globals.Service.Log(LogLevel.Debug, @"Connection {0} closed - {1} messages handled", connection.ConnectionId, connection.MessageCounter)
        .Wait();
    }

  }
}
