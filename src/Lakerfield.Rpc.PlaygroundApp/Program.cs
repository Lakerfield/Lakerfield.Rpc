using System;
using System.IO;
using Lakerfield.RpcTest;

namespace Lakerfield.Rpc;

public static class Program
{
  public static void Main(string[] args)
  {
    Console.WriteLine("Hello");

    //var x = new SampleRpcServiceImplementation();

    try
    {
      DrieNulSendMessage<RpcMessage> message = new DrieNulSendMessage<RpcMessage>()
      {
        Opcode = MessageOpcode.PingRequest,
        Flags = MessageFlags.None,
        RequestId = 42,
        ResponseTo = 7,
        ObservableId = 9,
        Message = new CompanyFindByIdRequest()
        {
          Id = Guid.NewGuid()
        },
        //MessageLength = 
      };
      using var stream = new MemoryStream();

      message.WriteTo(stream);

      stream.Position = 0;

      DrieNulReceiveMessage<RpcMessage> receive = new DrieNulReceiveMessage<RpcMessage>();
      receive.ReadFrom(stream);

      Console.WriteLine(receive.Opcode);
      Console.WriteLine(receive.Flags);
      Console.WriteLine(receive.RequestId);
      Console.WriteLine(receive.ResponseTo);
      Console.WriteLine(receive.ObservableId);
      Console.WriteLine(receive.Message?.GetType().Name);

      //SendMessage(stream, message.RequestId);

    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }

  }
}


//[RpcService]
public interface IMyService
{
  void DoWork();
  int Calculate(int value);
}
