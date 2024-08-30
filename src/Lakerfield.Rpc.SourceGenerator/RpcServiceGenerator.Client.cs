using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.Rpc;

public partial class RpcServiceGenerator
{


  private void GenerateClientClass(SourceProductionContext context, INamedTypeSymbol classSymbol, bool hasServer, bool hasClient)
  {
    var className = classSymbol.Name;
    var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
    var serviceSymbol = classSymbol.Interfaces.FirstOrDefault();
    var serviceNamespaceName = serviceSymbol.ContainingNamespace.ToDisplayString();
    var bsonClassName = $"{serviceSymbol.Name.TrimStart('I')}";

    var sourceBuilder = new StringBuilder();
    var methodSourceBuilder = new StringBuilder();

    if (!hasClient)
      sourceBuilder.Append($$"""
                             #error {{className}} needs a reference to the Lakerfield.Rpc.Client package

                             """);

    // Implement each method from the interface
    //foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
    foreach (var member in GetAllInterfaceMembersIncludingInherited(classSymbol).OfType<IMethodSymbol>())
    {
      var isTask = member.ReturnType.Name == "Task";
      var isObservable = member.ReturnType.Name == "IObservable";
      if (!isTask && !isObservable)
        continue;

      var methodName = member.Name;
      var returnType = member.ReturnType.ToDisplayString();
      var returnTypeGenericType1 = GetGenericTypeArgument(member.ReturnType);
      var parameters = string.Join(", ", member.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
      var parameters2 = string.Join(", ", member.Parameters.Select(p => $"{CapitalizeFirstLetter(p.Name)} = {p.Name}"));

      if (isTask)
        methodSourceBuilder
          .Append($$"""
                        public async {{returnType}} {{methodName}}({{parameters}})
                        {
                          var request = new {{methodName}}Request() { {{parameters2}} };
                          var response = await Client.Execute<{{methodName}}Response>(request).ConfigureAwait(false);
                          {{(returnTypeGenericType1 == null ? "" : "return response.Result;")}}
                        }


                    """);

      if (isObservable)
        methodSourceBuilder
          .Append($$"""
                        public {{returnType}} {{methodName}}({{parameters}})
                        {
                          var request = new {{methodName}}Request() { {{parameters2}} };
                          var result = Client.ExecuteObservable<{{returnTypeGenericType1}}>(request);
                          return result;
                        }


                    """);

      //, CancellationToken cancellationToken = default
    }

    sourceBuilder.Append($$"""
      using System;
      using {{serviceNamespaceName}};

      namespace {{namespaceName}}
      {
      // server {{hasServer}} client {{hasClient}}
        public partial class {{className}}
        {
          public Lakerfield.Rpc.NetworkClient Client { get; }
          public {{className}}(Lakerfield.Rpc.NetworkClient client)
          {
            {{bsonClassName}}BsonConfigurator.Configure();
            Client = client;
          }

          {{methodSourceBuilder.ToString()}}
        }
      }

      """);

    // Add the generated source
    context.AddSource($"{className}.client.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
  }

}
