using System;

namespace Lakerfield.Rpc
{
  [Flags]
  public enum MessageFlags
  {
    None = 0,
    Broadcast = 1,
    Exception = 2,
    UnknownMessage = 4,
    //AwaitCapable = 8
  }
}
