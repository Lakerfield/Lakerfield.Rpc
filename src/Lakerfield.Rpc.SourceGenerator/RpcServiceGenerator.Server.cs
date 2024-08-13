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

    var nestedClassSymbol = classSymbol.GetTypeMembers().FirstOrDefault(t => t.Name == $"ClientConnectionMessageHandler");

    var sourceBuilder = new StringBuilder();
    var taskSwitchSourceBuilder = new StringBuilder();
    var observableSwitchSourceBuilder = new StringBuilder();
    var methodSourceBuilder = new StringBuilder();

    if (classSymbol.BaseType?.Name != "LakerfieldRpcServer")
      sourceBuilder.Append($$"""
                             #error {{className}} should inherit from Lakerfield.Rpc.LakerfieldRpcServer<IMyService>

                             """);

    var serviceSymbol = classSymbol.BaseType?.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
    var serviceNamespaceName = serviceSymbol.ContainingNamespace.ToDisplayString();
    var bsonClassName = $"{serviceSymbol.Name.TrimStart('I')}";


    // Implement each method from the interface
    //foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
    foreach (var member in GetAllInterfaceMembersIncludingInherited(serviceSymbol).OfType<IMethodSymbol>())
    {
      var isTask = member.ReturnType.Name == "Task";
      var isObservable = member.ReturnType.Name == "IObservable";
      if (!isTask && !isObservable)
        continue;

      var methodName = member.Name;
      var returnType = member.ReturnType.ToDisplayString();
      var returnTypeGenericType1 = GetGenericTypeArgument(member.ReturnType);
      var parameters = string.Join(", ", member.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
      var switchParameters = string.Join(", ", member.Parameters.Select(p => $"request.{CapitalizeFirstLetter(p.Name)}"));

      if (isTask)
        taskSwitchSourceBuilder
          .Append($$"""
                              {{methodName}}Request request => _{{methodName}}(request),

                    """);
      if (isObservable)
        observableSwitchSourceBuilder
          .Append($$"""
                              {{methodName}}Request request => _{{methodName}}(request),

                    """);

      //Log(context, $"HasMethod {methodName}, {nestedClassSymbol}");
      var hasMethod = HasMethod(nestedClassSymbol, methodName);
      if (!hasMethod && isTask)
        methodSourceBuilder
          .Append($$"""
                          #warning {{methodName}} of {{serviceSymbol.Name}} is not implemented
                          public {{returnType}} {{methodName}}({{parameters}})
                          {
                            throw new NotImplementedException("{{methodName}} of {{serviceSymbol.Name}} is not implemented");
                          }

                    """);
      else if (!hasMethod && isObservable)
        methodSourceBuilder
          .Append($$"""
                          #warning {{methodName}} of {{serviceSymbol.Name}} is not implemented
                          public IObservable<{{returnTypeGenericType1}}> {{methodName}}({{parameters}})
                          {
                            throw new NotImplementedException("{{methodName}} of {{serviceSymbol.Name}} is not implemented");
                          }

                    """);
      else
        methodSourceBuilder.AppendLine($"// {methodName} already implemented");

      if (isTask)
        methodSourceBuilder
          .Append($$"""
                          [EditorBrowsable(EditorBrowsableState.Never)]
                          public async Task<Lakerfield.Rpc.RpcMessage> _{{methodName}}({{methodName}}Request request)
                          {
                            return new {{methodName}}Response()
                            {
                              Result = await {{methodName}}({{switchParameters}}).ConfigureAwait(false)
                            };
                          }

                    """);

      if (isObservable)
        methodSourceBuilder
          .Append($$"""
                          [EditorBrowsable(EditorBrowsableState.Never)]
                          public Lakerfield.Rpc.NetworkObservable _{{methodName}}({{methodName}}Request request)
                          {
                            return new Lakerfield.Rpc.NetworkObservable<{{returnTypeGenericType1}}>({{methodName}}({{switchParameters}}));
                          }

                    """);

      //, CancellationToken cancellationToken = default
    }

    sourceBuilder.Append($$"""
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using {{serviceNamespaceName}};

namespace {{namespaceName}}
{
  // server {{hasServer}} client {{hasClient}} from {{serviceSymbol.ToDisplayString()}}
  public partial class {{className}}
  {
    public {{className}}(IPAddress ipAddress, int port) : base (ipAddress, port)
    {
    }

    public override void InitBsonClassMaps()
    {
      {{bsonClassName}}BsonConfigurator.Configure();
    }

    //public override Lakerfield.Rpc.ILakerfieldRpcClientMessageHandler CreateConnectionMessageRouter(Lakerfield.Rpc.LakerfieldRpcServerConnection connection)
    //{
    //  return new Lakerfield.Rpc.LakerfieldRpcMessageRouter(connection);
    //}

    public partial class ClientConnectionMessageHandler : Lakerfield.Rpc.ILakerfieldRpcClientMessageHandler
    {
      public Lakerfield.Rpc.LakerfieldRpcServerConnection Connection { get; }

      public ClientConnectionMessageHandler(Lakerfield.Rpc.LakerfieldRpcServerConnection connection)
      {
        Connection = connection;
      }

      public Task<Lakerfield.Rpc.RpcMessage> HandleMessage(Lakerfield.Rpc.RpcMessage message)
      {
        if (message == null)
          throw new ArgumentNullException("message", "Cannot route null RpcMessage");

System.Console.WriteLine($"new message {message.GetType().Name}");
        return message switch {
{{taskSwitchSourceBuilder.ToString()}}
          _ => TaskNotImplementedMessage(message)
        };
      }

      public Lakerfield.Rpc.NetworkObservable HandleObservable(Lakerfield.Rpc.RpcMessage message)
      {
        if (message == null)
          throw new ArgumentNullException("message", "Cannot route null RpcMessage");

System.Console.WriteLine($"new message {message.GetType().Name}");
        return message switch {
{{observableSwitchSourceBuilder.ToString()}}
          _ => ObservableNotImplementedMessage(message)
        };
      }

      private Task<Lakerfield.Rpc.RpcMessage> TaskNotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
      {
        throw new NotImplementedException(string.Format("Message {0} not implemented", message.GetType().Name));
      }

      private Lakerfield.Rpc.NetworkObservable ObservableNotImplementedMessage(Lakerfield.Rpc.RpcMessage message)
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
