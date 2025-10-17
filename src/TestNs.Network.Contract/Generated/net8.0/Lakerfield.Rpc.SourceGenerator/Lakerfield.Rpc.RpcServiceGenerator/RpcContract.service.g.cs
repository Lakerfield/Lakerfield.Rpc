using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TestNs.Network.Contract
{
  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageHelloHelloRequest : Lakerfield.Rpc.RpcMessage
  {

  }

  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageHelloHelloResponse: Lakerfield.Rpc.RpcMessage
  {
    public bool Result { get; set; }
  }



  //[EditorBrowsable(EditorBrowsableState.Never)]
  public class RpcMessageLoginRequest : Lakerfield.Rpc.RpcMessage
  {
    public TestNs.Network.Models.LoginRequest _TheRequest { get; set; }

  }



  public static partial class RpcContractBsonConfigurator
  {

    private static bool _configured = false;
    public static void Configure()
    {
      if (_configured)
        return;

      _configured = true;

      PreConfigure();

      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageHelloHelloRequest>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageHelloHelloResponse>(AutoMap);
      Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessageLoginRequest>(AutoMap);

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
