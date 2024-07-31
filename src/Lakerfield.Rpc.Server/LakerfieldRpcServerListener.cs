﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lakerfield.Rpc
{
  public class LakerfieldRpcServerListener
  {
    private readonly LakerfieldRpcMessageRouterFactory _messageRouterFactory;
    private readonly TcpListener _tcpListener;
    private Task _acceptNewClientsTask;
    private readonly List<LakerfieldRpcServerConnection> _connections = new List<LakerfieldRpcServerConnection>();

    public LakerfieldRpcServerListener(LakerfieldRpcMessageRouterFactory messageRouterFactory, IPAddress ipAddress, int port)
    {
      _messageRouterFactory = messageRouterFactory;
      _tcpListener = new TcpListener(ipAddress, port);
    }

    public LakerfieldRpcServerConnection[] Connections
    {
      get
      {
        lock (_connections)
          return _connections.ToArray();
      }
    }

    public bool AcceptNewClientsRunning
    {
      get { return !_acceptNewClientsTask.IsCompleted; }
    }

    public void Start()
    {
      _tcpListener.Start();

      _acceptNewClientsTask = AcceptNewClientsLoopAsync();
    }

    public void Stop()
    {
      _tcpListener.Stop();
    }


    private async Task AcceptNewClientsLoopAsync()
    {
      try
      {
        while (true)
        {
          Console.WriteLine("Waiting for a connection... ");
          var tcpClient = await _tcpListener.AcceptTcpClientAsync();

          var connection = new LakerfieldRpcServerConnection(
            tcpClient,
            connection => _messageRouterFactory.CreateRouter(connection),
            this);
          Console.WriteLine(@"Connection {0} opened", connection.ConnectionId);
          lock (_connections)
            _connections.Add(connection);
        }
      }
      catch (SocketException ex)
      {
        Console.WriteLine(@"Listener loop {0}", ex.Message);
      }
    }

    internal void Cleanup(LakerfieldRpcServerConnection connection)
    {
      Console.WriteLine(@"Connection {0} closed - {1} messages handled", connection.ConnectionId, connection.MessageCounter);
      lock (_connections)
        _connections.Remove(connection);
      Globals.Service.Log(LogLevel.Debug, @"Connection {0} closed - {1} messages handled", connection.ConnectionId, connection.MessageCounter)
        .Wait();
    }


    static LakerfieldRpcServerListener()
    {
      BsonConfiguration.Configure();
    }


  }
}
