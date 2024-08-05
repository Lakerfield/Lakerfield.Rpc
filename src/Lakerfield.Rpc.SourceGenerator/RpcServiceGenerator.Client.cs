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
    var bsonClassName = TrimSuffix(className, "Client");
    var sourceBuilder = new StringBuilder($$"""
                                            using System;

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

                                            """);

    // Implement each method from the interface
    //foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
    foreach (var member in GetAllInterfaceMembersIncludingInherited(classSymbol).OfType<IMethodSymbol>())
    {
      var methodName = member.Name;
      var returnType = member.ReturnType.ToDisplayString();
      var returnTypeExTask = GetGenericTypeArgument(member.ReturnType);
      var parameters = string.Join(", ", member.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
      var parameters2 = string.Join(", ", member.Parameters.Select(p => $"{CapitalizeFirstLetter(p.Name)} = {p.Name}"));

      sourceBuilder.Append($$"""
                                 public async {{returnType}} {{methodName}}({{parameters}})
                                 {
                                   var request = new {{methodName}}Request() { {{parameters2}} };
                                   var response = await Client.Execute<{{methodName}}Response>(request).ConfigureAwait(false);
                                   {{(returnTypeExTask == null ? "" : "return response.Result;")}}
                                 }


                             """);

      //, CancellationToken cancellationToken = default
    }

    sourceBuilder.Append("""
                          }
                         }

                         """);

    // Add the generated source
    context.AddSource($"{className}.client.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
  }

}
