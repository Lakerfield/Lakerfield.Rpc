using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RpcSample
{
  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanyFindByIdRequest : Lakerfield.Rpc.RpcMessage
  {
    public System.Guid _Id { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanyFindByIdResponse: Lakerfield.Rpc.RpcMessage
  {
    public RpcSample.Models.Company Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanyFindAllRequest : Lakerfield.Rpc.RpcMessage
  {

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanyFindAllResponse: Lakerfield.Rpc.RpcMessage
  {
    public RpcSample.Models.Company[] Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanySaveRequest : Lakerfield.Rpc.RpcMessage
  {
    public RpcSample.Models.Company _Entity { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanySaveResponse: Lakerfield.Rpc.RpcMessage
  {
    public RpcSample.Models.Company Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanyDeleteRequest : Lakerfield.Rpc.RpcMessage
  {
    public RpcSample.Models.Company _Entity { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanyDeleteResponse: Lakerfield.Rpc.RpcMessage
  {
    public bool Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanyTestRequest : Lakerfield.Rpc.RpcMessage
  {
    public RpcSample.Models.Company _Entity { get; set; }
    public RpcSample.Models.Company _Entity2 { get; set; }
    public string _Wouter { get; set; }
    public int _Bert { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageCompanyTestResponse: Lakerfield.Rpc.RpcMessage
  {
    public (RpcSample.Models.Company, string) Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageGetObservableRequest : Lakerfield.Rpc.RpcMessage
  {
    public System.Guid _Id { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageMyVoidTestRequest : Lakerfield.Rpc.RpcMessage
  {
    public RpcSample.Models.Company _Entity { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageMyVoidTestResponse: Lakerfield.Rpc.RpcMessage
  {
    
  }





  public static partial class RpcTestServiceBsonConfigurator
  {

    private static bool _configured = false;
    public static void Configure()
    {
      if (_configured)
        return;

      _configured = true;

      PreConfigure();

      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanyFindByIdRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanyFindByIdResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanyFindAllRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanyFindAllResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanySaveRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanySaveResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanyDeleteRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanyDeleteResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanyTestRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageCompanyTestResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageGetObservableRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageMyVoidTestRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageMyVoidTestResponse>(AutoMap);

      PostConfigure();
    }

    static partial void PreConfigure();
    static partial void PostConfigure();

    private static void AutoMap<T>(Lakerfield.Bson.Serialization.BsonClassMap<T> cm)
    {
      cm.AutoMap();
    }

    private static void AutoMapAndSetGenericDiscriminator(Lakerfield.Bson.Serialization.BsonClassMap cm)
    {
      cm.AutoMap();

      var cmType = cm.GetType();
      var cmGenericType = cmType.GenericTypeArguments.First();
      var discriminator = cmGenericType.Name;
      var cmGenericTypeType = cmGenericType.GenericTypeArguments.FirstOrDefault();
      if (cmGenericTypeType != null)
        discriminator += cmGenericTypeType.Name;
      cm.SetDiscriminator(discriminator);
    }

  }

}
