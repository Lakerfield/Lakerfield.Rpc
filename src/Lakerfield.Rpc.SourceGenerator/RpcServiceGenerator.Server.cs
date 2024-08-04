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

    var nestedClassSymbol = classSymbol.GetTypeMembers().FirstOrDefault(t => t.Name == $"{className}MessageRouter");

    var sourceBuilder = new StringBuilder();
    var switchSourceBuilder = new StringBuilder();
    var methodSourceBuilder = new StringBuilder();

    if (classSymbol.BaseType?.Name != "LakerfieldRpcServer")
      sourceBuilder.Append($$"""
                             #error {{className}} should inherit from Lakerfield.Rpc.LakerfieldRpcServer<IMyService>

                             """);

    var serviceSymbol = classSymbol.BaseType?.TypeArguments.FirstOrDefault() as INamedTypeSymbol;


    // Implement each method from the interface
    //foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
    foreach (var member in GetAllInterfaceMembersIncludingInherited(serviceSymbol).OfType<IMethodSymbol>())
    {
      var methodName = member.Name;
      var returnType = member.ReturnType.ToDisplayString();
      var returnTypeExTask = GetGenericTypeArgument(member.ReturnType);
      var parameters = string.Join(", ", member.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
      var switchParameters = string.Join(", ", member.Parameters.Select(p => $"request._{p.Name}"));

      switchSourceBuilder.Append($$"""
                                             {{methodName}}Request request => _{{methodName}}(request),

                                   """);

      var methodPropertiesSourceBuilder = new StringBuilder();
      foreach (var memberParameter in member.Parameters)
      {
        methodPropertiesSourceBuilder.AppendLine($"        public {memberParameter.Type} _{memberParameter.Name} {{ get; set; }}");
      }

      if (!HasMethod(nestedClassSymbol, methodName))
        methodSourceBuilder.Append($$"""
                                           public {{returnType}} {{methodName}}({{parameters}})
                                           {
                                             throw new NotImplementedException();
                                           }

                                     """);

      methodSourceBuilder.Append($$"""
                                         [EditorBrowsable(EditorBrowsableState.Never)]
                                         public async Task<Lakerfield.Rpc.RpcMessage> _{{methodName}}({{methodName}}Request request)
                                         {
                                           return new {{methodName}}Response()
                                           {
                                             Result = await {{methodName}}({{switchParameters}}).ConfigureAwait(false)
                                           };
                                         }
                                         [EditorBrowsable(EditorBrowsableState.Never)]
                                         public class {{methodName}}Request : Lakerfield.Rpc.RpcMessage
                                         {
                                   {{methodPropertiesSourceBuilder.ToString()}}
                                         }
                                         [EditorBrowsable(EditorBrowsableState.Never)]
                                         public class {{methodName}}Response: Lakerfield.Rpc.RpcMessage
                                         {
                                           public {{returnTypeExTask}} Result { get; set; }
                                         }


                                   """);
      //, CancellationToken cancellationToken = default
    }

    sourceBuilder.Append($$"""
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace {{namespaceName}}
{
  // server {{hasServer}} client {{hasClient}}
  public partial class {{className}}
  {
    public {{className}}(IPAddress ipAddress, int port) : base (ipAddress, port)
    {
    }

    public override Lakerfield.Rpc.ILakerfieldRpcMessageRouter CreateConnectionMessageRouter(LakerfieldRpcServerConnection connection)
    {
      return new Lakerfield.Rpc.LakerfieldRpcMessageRouter(connection);
    }

    public partial class {{className}}MessageRouter : Lakerfield.Rpc.ILakerfieldRpcMessageRouter
    {
      public Lakerfield.Rpc.LakerfieldRpcServerConnection Connection { get; }

      public void {{className}}MessageRouter(Lakerfield.Rpc.LakerfieldRpcServerConnection connection)
      {
        Connection = connection;
      }

      public Task<Lakerfield.Rpc.RpcMessage> HandleMessage(Lakerfield.Rpc.RpcMessage message)
      {
        if (message == null)
          throw new ArgumentNullException("message", "Cannot route null RpcMessage");

        return message switch {
{{switchSourceBuilder.ToString()}}
          _ => NotImplementedMessage(message)
        };
      }

      private Task<Lakerfield.Rpc.RpcMessage> NotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
      {
        throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
      }

{{methodSourceBuilder.ToString()}}

    }
  }
}

""");

    // Add the generated source
    context.AddSource($"{className}.server.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
  }

}
