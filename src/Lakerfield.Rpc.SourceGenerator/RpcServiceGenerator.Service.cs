using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.Rpc;

public partial class RpcServiceGenerator
{


  private void GenerateServiceClasses(SourceProductionContext context, INamedTypeSymbol interfaceSymbol, bool hasServer, bool hasClient)
  {
    var interfaceName = interfaceSymbol.Name;
    var namespaceName = interfaceSymbol.ContainingNamespace.ToDisplayString();
    var className = $"{interfaceName.TrimStart('I')}";

    var sourceBuilder = new StringBuilder();
    var requestResponseModelsSourceBuilder = new StringBuilder();
    var bsonClassMapsSourceBuilder = new StringBuilder();

    // Implement each method from the interface
    //foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
    foreach (var member in GetAllInterfaceMembersIncludingInherited(interfaceSymbol).OfType<IMethodSymbol>())
    {
      var methodName = member.Name;
      var returnType = member.ReturnType.ToDisplayString();

      var isTask = member.ReturnType.Name == "Task";
      var isObservable = member.ReturnType.Name == "IObservable";

      if (!isTask && !isObservable)
      {
        requestResponseModelsSourceBuilder
          .Append($$"""
                    #warning Return type of {{interfaceName}}.{{methodName}} is not Task or IObservable but {{member.ReturnType.Name}}


                    """);
        continue;
      }

      var returnTypeExTask = GetGenericTypeArgument(member.ReturnType);

      var methodPropertiesSourceBuilder = new StringBuilder();
      foreach (var memberParameter in member.Parameters)
      {
        methodPropertiesSourceBuilder.AppendLine(
          $"        public {memberParameter.Type} {CapitalizeFirstLetter(memberParameter.Name)} {{ get; set; }}");
      }

      if (isTask || isObservable)
      {  requestResponseModelsSourceBuilder
          .Append($$"""
                          //[EditorBrowsable(EditorBrowsableState.Never)]
                          public class {{methodName}}Request : Lakerfield.Rpc.RpcMessage
                          {
                    {{methodPropertiesSourceBuilder.ToString()}}
                          }

                    """);
        bsonClassMapsSourceBuilder
          .Append($$"""
                          Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<{{methodName}}Request>(AutoMap);

                    """);
      }

      if (isTask)
      {
        requestResponseModelsSourceBuilder
          .Append($$"""
                          //[EditorBrowsable(EditorBrowsableState.Never)]
                          public class {{methodName}}Response: Lakerfield.Rpc.RpcMessage
                          {
                            public {{returnTypeExTask}} Result { get; set; }
                          }


                    """);
        bsonClassMapsSourceBuilder
          .Append($$"""
                          Lakerfield.Bson.Serialization.BsonClassMap.RegisterClassMap<{{methodName}}Response>(AutoMap);

                    """);
      }

      //, CancellationToken cancellationToken = default
    }

    sourceBuilder.Append($$"""
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace {{namespaceName}}
{
  // server {{hasServer}} client {{hasClient}}

{{requestResponseModelsSourceBuilder.ToString()}}

  public static partial class {{className}}BsonConfigurator
  {

    private static bool _configured = false;
    public static void Configure()
    {
      if (_configured)
        return;

      _configured = true;

      PreConfigure();
{{bsonClassMapsSourceBuilder.ToString()}}
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

""");

    // Add the generated source
    context.AddSource($"{className}.service.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
  }

}
