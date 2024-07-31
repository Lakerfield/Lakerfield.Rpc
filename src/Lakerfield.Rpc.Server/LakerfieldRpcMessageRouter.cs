using System;
using System.Threading.Tasks;

namespace Lakerfield.Rpc
{
  public class LakerfieldRpcMessageRouter : ILakerfieldRpcMessageRouter
  {
    public IClientExportService Service { get; private set; }
    public LakerfieldRpcServerConnection Connection { get; }

    protected EktMessageHandler Ekt { get; set; }

    public LakerfieldRpcMessageRouter(IClientExportService service, LakerfieldRpcServerConnection connection)
    {
      Service = service;
      Connection = connection;

      Ekt = new EktMessageHandler(connection, service.Ekt);

      SetupHandleMessageTypeSwitch();
      SetupHandleObservableTypeSwitch();
    }



    public Task<RpcMessage> HandleMessage(RpcMessage message)
    {
      if (message == null)
        throw new ArgumentNullException("message", "Cannot route null RpcMessage");

      return _handleMessageTypeSwitch.Do(message);
    }

    private TypeSwitchReturn<Task<Messages.NetworkMessage>> _handleMessageTypeSwitch;
    private void SetupHandleMessageTypeSwitch()
    {
      _handleMessageTypeSwitch = new TypeSwitchReturn<Task<Messages.NetworkMessage>>(
        //Ekt
          TypeSwitchReturn<Task<Messages.NetworkMessage>>.Case<Messages.FindByIdRequest<Ekt>>(Ekt.FindById),
          TypeSwitchReturn<Task<Messages.NetworkMessage>>.Case<Messages.FindAllRequest<Ekt>>(Ekt.FindAll),
          TypeSwitchReturn<Task<Messages.NetworkMessage>>.Case<Messages.SaveRequest<Ekt>>(Ekt.Save),
          TypeSwitchReturn<Task<Messages.NetworkMessage>>.Case<Messages.DeleteRequest<Ekt>>(Ekt.Delete),
        //Not implemented
          TypeSwitchReturn<Task<Messages.NetworkMessage>>.Default<Messages.NetworkMessage>(NotImplementedMessage)
        );
    }

    private Task<RpcMessage> NotImplementedMessage(RpcMessage message)
    {
      throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
    }



    public NetworkObservable HandleObservable(RpcMessage message)
    {
      if (message == null)
        throw new ArgumentNullException("message", "Cannot route null observable RpcMessage");

      return _handleObservableTypeSwitch.Do(message);
    }

    private TypeSwitchReturn<NetworkObservable> _handleObservableTypeSwitch;
    private void SetupHandleObservableTypeSwitch()
    {
      _handleObservableTypeSwitch = new TypeSwitchReturn<NetworkObservable>(
        //Ekt
          TypeSwitchReturn<NetworkObservable>.Case<Messages.EktGetObservableRequest>(Ekt.GetObservable),
        //Not implemented
          TypeSwitchReturn<NetworkObservable>.Default<Messages.NetworkMessage>(NotImplementedObservable)
        );
    }

    private NetworkObservable NotImplementedObservable(RpcMessage message)
    {
      throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
    }

  }
}
