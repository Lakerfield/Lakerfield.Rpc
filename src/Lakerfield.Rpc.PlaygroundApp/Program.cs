﻿using System;

namespace Lakerfield.Rpc;

public static class Program
{
  public static void Main(string[] args)
  {
    Console.WriteLine("Hello");

    
  }
}


[RpcService]
public interface IMyService
{
  void DoWork();
  int Calculate(int value);
}
