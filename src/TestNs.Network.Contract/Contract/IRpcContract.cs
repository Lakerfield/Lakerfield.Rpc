using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lakerfield.Bson.Serialization.Serializers;
using Lakerfield.Bson.Serialization;
using Lakerfield.Rpc;
using TestNs.Network.Models;

namespace TestNs.Network.Contract;

[RpcService]
public interface IRpcContract
{

  // hello
  Task<bool> HelloHello();



  // core
  IObservable<User> Login(Models.LoginRequest theRequest);

}

public static partial class RpcContractBsonConfigurator
{
  private static bool IsAllowedType(Type type)
  {
    return type.IsConstructedGenericType ?
      type.GetGenericArguments().All(IsAllowedType) :
      type.FullName.StartsWith("TestNs");
  }

  static partial void PreConfigure()
  {
    //var objectSerializer = new ObjectSerializer(ObjectSerializer.AllAllowedTypes);
    var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || IsAllowedType(type));
    BsonSerializer.RegisterSerializer(objectSerializer);

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcMessage>();
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcExceptionMessage>();
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<RpcObservableMessage>();

    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<Models.LoginRequest>(cm =>
    {
      cm.AutoMap();
      //cm.SetDiscriminator("Company");
    });
    Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<Models.User>(AutoMap);
  }

  static partial void PostConfigure()
  {

  }
}
