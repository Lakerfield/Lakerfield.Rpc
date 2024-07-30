namespace Lakerfield.Rpc
{
  public enum MessageOpcode
  {
    PingRequest = 1,
    PingReply = 2,

    SystemMessage = 99,

    MessageRequest = 1001,
    MessageResponse = 1002,

    ObservableRequest = 2001,
    ObservableOnNext = 2002,
    ObservableOnComplete = 2003,
    ObservableOnException = 2004,
    ObservableDispose = 2005,

  }
}
