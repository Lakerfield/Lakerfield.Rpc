using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using TestNs.Network.Contract;

namespace TestNs.Network.Server
{
  public partial class RpcTestNsServer // global::TestNs.Network.Contract.IRpcContract
  {
    public RpcTestNsServer() : base ()
    {
      InitBsonClassMaps();
    }

    public override void InitBsonClassMaps()
    {
      RpcContractBsonConfigurator.Configure();
    }

    //public override Lakerfield.Rpc.ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(Lakerfield.Rpc.LakerfieldRpcWebSocketServerConnection connection)
    //{
    //  return new Lakerfield.Rpc.LakerfieldRpcMessageRouter(connection);
    //}

    public partial class ClientConnectionMessageHandler : Lakerfield.Rpc.ILakerfieldRpcClientMessageHandler
    {
      public Lakerfield.Rpc.LakerfieldRpcWebSocketServerConnection Connection { get; }

      public ClientConnectionMessageHandler(Lakerfield.Rpc.LakerfieldRpcWebSocketServerConnection connection)
      {
        Connection = connection;
      }

      public Task<Lakerfield.Rpc.RpcMessage> HandleMessage(Lakerfield.Rpc.RpcMessage message)
      {
        if (message == null)
          throw new ArgumentNullException("message", "Cannot route null RpcMessage");

#if DEBUG
        System.Console.WriteLine($"new message {message.GetType().Name}");
#endif
        return message switch {
          RpcMessageHelloHelloRequest request => _HelloHello(request),

          _ => TaskNotImplementedMessage(message)
        };
      }

      private Task<Lakerfield.Rpc.RpcMessage> TaskNotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
      {
        throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
      }

      public Lakerfield.Rpc.NetworkObservable HandleObservable(Lakerfield.Rpc.RpcMessage message)
      {
        if (message == null)
          throw new ArgumentNullException("message", "Cannot route null RpcMessage");

#if DEBUG
        System.Console.WriteLine($"new message {message.GetType().Name}");
#endif
        return message switch {
          RpcMessageLoginRequest request => _Login(request),

          _ => ObservableNotImplementedMessage(message)
        };
      }

      private Lakerfield.Rpc.NetworkObservable ObservableNotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
      {
        throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
      }

      // HelloHello already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _HelloHello(RpcMessageHelloHelloRequest request)
      {
        return new RpcMessageHelloHelloResponse()
        {
          Result = await HelloHello().ConfigureAwait(false)
        };
      }

      // Login already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public Lakerfield.Rpc.NetworkObservable _Login(RpcMessageLoginRequest request)
      {
        return new Lakerfield.Rpc.NetworkObservable<TestNs.Network.Models.User>(Login(request._TheRequest));
      }



    }
  }
}
