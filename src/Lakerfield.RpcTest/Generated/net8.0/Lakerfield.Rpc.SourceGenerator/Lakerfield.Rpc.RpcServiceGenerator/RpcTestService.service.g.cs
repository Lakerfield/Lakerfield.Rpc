using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lakerfield.RpcTest
{
  // server False client False

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanyFindByIdRequest : Lakerfield.Rpc.RpcMessage
  {
    public System.Guid Id { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanyFindByIdResponse: Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RpcTest.Models.Company Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanyFindAllRequest : Lakerfield.Rpc.RpcMessage
  {

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanyFindAllResponse: Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RpcTest.Models.Company[] Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanySaveRequest : Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RpcTest.Models.Company Entity { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanySaveResponse: Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RpcTest.Models.Company Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanyDeleteRequest : Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RpcTest.Models.Company Entity { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanyDeleteResponse: Lakerfield.Rpc.RpcMessage
  {
    public bool Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanyTestRequest : Lakerfield.Rpc.RpcMessage
  {
    public Lakerfield.RpcTest.Models.Company Entity { get; set; }
    public Lakerfield.RpcTest.Models.Company Entity2 { get; set; }
    public string Wouter { get; set; }
    public int Bert { get; set; }

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class CompanyTestResponse: Lakerfield.Rpc.RpcMessage
  {
    public (Lakerfield.RpcTest.Models.Company, string) Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class GetObservableRequest : Lakerfield.Rpc.RpcMessage
  {
    public System.Guid Id { get; set; }

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

      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanyFindByIdRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanyFindByIdResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanyFindAllRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanyFindAllResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanySaveRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanySaveResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanyDeleteRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanyDeleteResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanyTestRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<CompanyTestResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<GetObservableRequest>(AutoMap);

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
