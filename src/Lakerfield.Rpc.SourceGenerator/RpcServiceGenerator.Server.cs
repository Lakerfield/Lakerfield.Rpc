using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.Rpc;

public partial class RpcServiceGenerator
{


  private void GenerateServerClass(SourceProductionContext context, INamedTypeSymbol classSymbol, bool hasServer, bool hasClient)
  {
    var className = classSymbol.Name;
    var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
    var sourceBuilder = new StringBuilder($$"""
                                            using System;

                                            namespace {{namespaceName}}
                                            {
                                              // server {{hasServer}} client {{hasClient}}
                                              public partial class {{className}} : Lakerfield.Rpc.LakerfieldRpcServerListener
                                              {
                                                public {{className}}(IPAddress ipAddress, int port) : base (new Lakerfield.Rpc.LakerfieldRpcMessageRouterFactory(), ipAddress, port)
                                                {
                                                }
                                              }

                                              public class {{className}}MessageRouterFactory : Lakerfield.Rpc.ILakerfieldRpcMessageRouterFactory
                                              {
                                                public Lakerfield.Rpc.ILakerfieldRpcMessageRouter Create()
                                                {
                                                  return new Lakerfield.Rpc.LakerfieldRpcMessageRouter();
                                                }
                                              }

                                              public partial class {{className}}MessageRouter : Lakerfield.Rpc.ILakerfieldRpcMessageRouter
                                              {
                                                public void RouteMessage(Lakerfield.Rpc.RpcMessage message)
                                                {
                                                  throw new NotImplementedException();
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

      sourceBuilder.Append($$"""
                                 public {{returnType}} {{methodName}}({{parameters}})
                                 {
                                   throw new NotImplementedException();//
                                 }

                                 public class {{methodName}}Request : Lakerfield.Rpc.RpcMessage
                                 {
                                   public {{member.Parameters.FirstOrDefault()?.Type}} X { get; set; }
                                 }
                                 public class {{methodName}}Response: Lakerfield.Rpc.RpcMessage
                                 {
                                   public {{returnTypeExTask}} Result { get; set; }
                                 }


                             """);
      //, CancellationToken cancellationToken = default
    }

    sourceBuilder.Append("""
                          }
                         }

                         """);

    // Add the generated source
    context.AddSource($"{className}.server.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
  }

}
