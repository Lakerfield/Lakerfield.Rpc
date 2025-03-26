using System;
using System.Net;
using System.Threading;
using RpcSample;

Console.WriteLine("Hello, World!");

var cancellation = new CancellationTokenSource();

var listener = new RpcTestServiceServer(new IPEndPoint(IPAddress.Loopback, 3000));

_ = listener.StartAsync(cancellation.Token);

Console.ReadKey();

cancellation.Cancel();


