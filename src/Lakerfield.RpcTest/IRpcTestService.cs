using System;
using System.Linq;
using Lakerfield.Bson.Serialization;
using Lakerfield.Bson.Serialization.Serializers;
using Lakerfield.Rpc;

namespace Lakerfield.RpcTest;

[RpcService]
public partial interface IRpcTestService
{
  // implementation in Services/...Service.cs

}

public static partial class RpcTestServiceBsonConfigurator
{
  private static bool IsAllowedType(Type type)
  {
    return type.IsConstructedGenericType ?
      type.GetGenericArguments().All(IsAllowedType) :
      type.FullName.StartsWith("Lakerfield.RpcTest");
  }

  static partial void PreConfigure()
  {
    System.Console.WriteLine("Pre bson");

    //var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
    var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || IsAllowedType(type));
    BsonSerializer.RegisterSerializer(objectSerializer);

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessage>();
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcExceptionMessage>();
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcObservableMessage>();

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<Models.Company>(cm =>
    {
      cm.AutoMap();
      //cm.SetDiscriminator("Company");
    });

  }
  static partial void PostConfigure()
  {

  }
}
