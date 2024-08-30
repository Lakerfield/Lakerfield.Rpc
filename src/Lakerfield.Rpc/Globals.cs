using System;
using System.Threading.Tasks;

namespace Lakerfield.Rpc;

public static class Globals
{
  public static GlobalsService Service { get; } = new GlobalsService();
}

public class GlobalsService
{
  public Task Log(LogLevel level, string message, params object[] ps)
  {

    return Task.CompletedTask;
  }
  public Task Log(LogLevel level, Exception exception, string message)
  {

    return Task.CompletedTask;
  }
}

public enum LogLevel
{
  Trace,
  Debug,
  Information,
  Warning,
  Error,
  Critical,
  None
}
