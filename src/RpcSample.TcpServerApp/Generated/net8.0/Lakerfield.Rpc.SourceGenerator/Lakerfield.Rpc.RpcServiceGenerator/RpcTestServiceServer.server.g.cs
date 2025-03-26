using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using RpcSample;

namespace RpcSample
{
  // server True client False from RpcSample.IRpcTestService
  public partial class RpcTestServiceServer
  {
    public RpcTestServiceServer(IPEndPoint endPoint) : base (endPoint)
    {
    }

    public override void InitBsonClassMaps()
    {
      RpcTestServiceBsonConfigurator.Configure();
    }

    //public override Lakerfield.Rpc.ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(Lakerfield.Rpc.LakerfieldRpcServerConnection connection)
    //{
    //  return new Lakerfield.Rpc.LakerfieldRpcMessageRouter(connection);
    //}

    public partial class ClientConnectionMessageHandler : Lakerfield.Rpc.ILakerfieldRpcClientMessageHandler
    {
      public Lakerfield.Rpc.LakerfieldRpcServerConnection Connection { get; }

      public ClientConnectionMessageHandler(Lakerfield.Rpc.LakerfieldRpcServerConnection connection)
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
          CompanyFindByIdRequest request => _CompanyFindById(request),
          CompanyFindAllRequest request => _CompanyFindAll(request),
          CompanySaveRequest request => _CompanySave(request),
          CompanyDeleteRequest request => _CompanyDelete(request),
          CompanyTestRequest request => _CompanyTest(request),

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
          GetObservableRequest request => _GetObservable(request),

          _ => ObservableNotImplementedMessage(message)
        };
      }

      private Lakerfield.Rpc.NetworkObservable ObservableNotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
      {
        throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
      }

      // CompanyFindById already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanyFindById(CompanyFindByIdRequest request)
      {
        return new CompanyFindByIdResponse()
        {
          Result = await CompanyFindById(request.Id).ConfigureAwait(false)
        };
      }

      #warning CompanyFindAll of IRpcTestService is not implemented
      public System.Threading.Tasks.Task<RpcSample.Models.Company[]> CompanyFindAll()
      {
        throw new NotImplementedException("CompanyFindAll of IRpcTestService is not implemented");
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanyFindAll(CompanyFindAllRequest request)
      {
        return new CompanyFindAllResponse()
        {
          Result = await CompanyFindAll().ConfigureAwait(false)
        };
      }

      #warning CompanySave of IRpcTestService is not implemented
      public System.Threading.Tasks.Task<RpcSample.Models.Company> CompanySave(RpcSample.Models.Company entity)
      {
        throw new NotImplementedException("CompanySave of IRpcTestService is not implemented");
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanySave(CompanySaveRequest request)
      {
        return new CompanySaveResponse()
        {
          Result = await CompanySave(request.Entity).ConfigureAwait(false)
        };
      }

      #warning CompanyDelete of IRpcTestService is not implemented
      public System.Threading.Tasks.Task<bool> CompanyDelete(RpcSample.Models.Company entity)
      {
        throw new NotImplementedException("CompanyDelete of IRpcTestService is not implemented");
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanyDelete(CompanyDeleteRequest request)
      {
        return new CompanyDeleteResponse()
        {
          Result = await CompanyDelete(request.Entity).ConfigureAwait(false)
        };
      }

      #warning CompanyTest of IRpcTestService is not implemented
      public System.Threading.Tasks.Task<(RpcSample.Models.Company, string)> CompanyTest(RpcSample.Models.Company entity, RpcSample.Models.Company entity2, string wouter, int bert)
      {
        throw new NotImplementedException("CompanyTest of IRpcTestService is not implemented");
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      public async Task<Lakerfield.Rpc.RpcMessage> _CompanyTest(CompanyTestRequest request)
      {
        return new CompanyTestResponse()
        {
          Result = await CompanyTest(request.Entity, request.Entity2, request.Wouter, request.Bert).ConfigureAwait(false)
        };
      }

      // GetObservable already implemented
      [EditorBrowsable(EditorBrowsableState.Never)]
      public Lakerfield.Rpc.NetworkObservable _GetObservable(GetObservableRequest request)
      {
        return new Lakerfield.Rpc.NetworkObservable<RpcSample.Models.Company>(GetObservable(request.Id));
      }



    }
  }
}
