using System;
using Lakerfield.Bson.Serialization;

namespace Lakerfield.Rpc;

public static class Program
{
  public static void Main(string[] args)
  {
    Console.WriteLine("Hello");

    //var x = new SampleRpcServiceImplementation();

  }
}


//[RpcService]
public interface IMyService
{
  void DoWork();
  int Calculate(int value);
}
