using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using RpcSample;

namespace RpcSample
{
  public partial class RpcTestWebSocketServiceServer // global::RpcSample.IRpcTestService
  {
    public RpcTestWebSocketServiceServer() : base ()
    {
      InitBsonClassMaps();
    }

    public override void InitBsonClassMaps()
    {
      RpcTestServiceBsonConfigurator.Configure();
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
          RpcMessageCompanyFindByIdRequest request => _CompanyFindById(request),
          RpcMessageCompanyFindAllRequest request => _CompanyFindAll(request),
          RpcMessageCompanySaveRequest request => _CompanySave(request),
          RpcMessageCompanyDeleteRequest request => _CompanyDelete(request),
          RpcMessageCompanyTestRequest request => _CompanyTest(request),
          RpcMessageMyVoidTestRequest request => _MyVoidTest(request),

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
          RpcMessageGetObservableRequest request => _GetObservable(request),

          _ => ObservableNotImplementedMessage(message)
        };
      }

      private Lakerfield.Rpc.NetworkObservable ObservableNotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
      {
        throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
      }

      // CompanyFindById already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanyFindById(RpcMessageCompanyFindByIdRequest request)
      {
        return new RpcMessageCompanyFindByIdResponse()
        {
          Result = await CompanyFindById(request._Id).ConfigureAwait(false)
        };
      }

      #warning CompanyFindAll of IRpcTestService is not implemented
      public global::System.Threading.Tasks.Task<global::RpcSample.Models.Company[]> CompanyFindAll()
      {
        throw new NotImplementedException("CompanyFindAll of IRpcTestService is not implemented");
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanyFindAll(RpcMessageCompanyFindAllRequest request)
      {
        return new RpcMessageCompanyFindAllResponse()
        {
          Result = await CompanyFindAll().ConfigureAwait(false)
        };
      }

      #warning CompanySave of IRpcTestService is not implemented
      public global::System.Threading.Tasks.Task<global::RpcSample.Models.Company> CompanySave(global::RpcSample.Models.Company entity)
      {
        throw new NotImplementedException("CompanySave of IRpcTestService is not implemented");
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanySave(RpcMessageCompanySaveRequest request)
      {
        return new RpcMessageCompanySaveResponse()
        {
          Result = await CompanySave(request._Entity).ConfigureAwait(false)
        };
      }

      #warning CompanyDelete of IRpcTestService is not implemented
      public global::System.Threading.Tasks.Task<bool> CompanyDelete(global::RpcSample.Models.Company entity)
      {
        throw new NotImplementedException("CompanyDelete of IRpcTestService is not implemented");
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanyDelete(RpcMessageCompanyDeleteRequest request)
      {
        return new RpcMessageCompanyDeleteResponse()
        {
          Result = await CompanyDelete(request._Entity).ConfigureAwait(false)
        };
      }

      #warning CompanyTest of IRpcTestService is not implemented
      public global::System.Threading.Tasks.Task<(global::RpcSample.Models.Company, string)> CompanyTest(global::RpcSample.Models.Company entity, global::RpcSample.Models.Company entity2, string wouter, int bert)
      {
        throw new NotImplementedException("CompanyTest of IRpcTestService is not implemented");
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanyTest(RpcMessageCompanyTestRequest request)
      {
        return new RpcMessageCompanyTestResponse()
        {
          Result = await CompanyTest(request._Entity, request._Entity2, request._Wouter, request._Bert).ConfigureAwait(false)
        };
      }

      // GetObservable already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public Lakerfield.Rpc.NetworkObservable _GetObservable(RpcMessageGetObservableRequest request)
      {
        return new Lakerfield.Rpc.NetworkObservable<RpcSample.Models.Company>(GetObservable(request._Id));
      }

      // MyVoidTest already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _MyVoidTest(RpcMessageMyVoidTestRequest request)
      {
        await MyVoidTest(request._Entity).ConfigureAwait(false);
        return new RpcMessageMyVoidTestResponse();
      }



    }
  }
}
