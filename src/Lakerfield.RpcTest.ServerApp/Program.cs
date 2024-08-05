using System;
using System.Net;
using Lakerfield.RpcTest;

Console.WriteLine("Hello, World!");

var listener = new RpcTestServiceServer(
  IPAddress.Loopback,
  3000);

listener.Start();

Console.ReadKey();

listener.Stop();


