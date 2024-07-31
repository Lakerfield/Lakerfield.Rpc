using System;
using System.Net;
using Lakerfield.Rpc;

Console.WriteLine("Hello, World!");

var listener = new Lakerfield.Rpc.LakerfieldRpcServerListener(
  new LakerfieldRpcMessageRouterFactory(),
  IPAddress.Loopback,
  3000);

listener.Start();

Console.ReadKey();

listener.Stop();


