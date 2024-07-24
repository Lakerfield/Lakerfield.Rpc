using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lakerfield.Rpc;

[Generator]
public class RpcServiceGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    context.RegisterPostInitializationOutput(static postInitializationContext => {
      postInitializationContext.AddSource("RpcServiceAttribute.Generated.cs", SourceText.From("""
        using System;

        namespace Lakerfield.Rpc
        {
            internal sealed class RpcServiceAttribute : Attribute
            {
            }
        }
        """, Encoding.UTF8));
    });
  }


  public void Initialize(GeneratorInitializationContext context)
  {


    context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
  }

  public void Execute(GeneratorExecutionContext context)
  {
    if (context.SyntaxReceiver is not SyntaxReceiver receiver)
      return;

    var compilation = context.Compilation;

    foreach (var interfaceDeclaration in receiver.CandidateInterfaces)
    {
      var model = compilation.GetSemanticModel(interfaceDeclaration.SyntaxTree);
      var symbol = model.GetDeclaredSymbol(interfaceDeclaration);

      if (symbol == null || !symbol.GetAttributes().Any(ad => ad.AttributeClass?.ToDisplayString() == "RpcServiceAttribute"))
        continue;

      var namespaceName = symbol.ContainingNamespace.ToDisplayString();
      var interfaceName = symbol.Name;
      var abstractClassName = interfaceName.TrimStart('I'); // Simple heuristic to name the abstract class
      var source = GenerateAbstractClass(namespaceName, interfaceName, abstractClassName, symbol);

      context.AddSource($"{abstractClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
    }
  }

  private string GenerateAbstractClass(string namespaceName, string interfaceName, string abstractClassName,
    INamedTypeSymbol interfaceSymbol)
  {
    var methods = new StringBuilder();

    foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
    {
      var returnType = member.ReturnType.ToDisplayString();
      var methodName = member.Name;
      var parameters = string.Join(", ", member.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
      var defaultBody = returnType == "void" ? "" : " => default;";

      methods.AppendLine($@"
     public virtual {returnType} {methodName}({parameters}){defaultBody}
     {{
         throw new NotImplementedException();
     }}");
    }

    return $@"
using System;

namespace {namespaceName}
{{
public abstract class {abstractClassName} : {interfaceName}
{{
{methods}
}}
}}";
  }

  private class SyntaxReceiver : ISyntaxReceiver
  {
    public List<InterfaceDeclarationSyntax> CandidateInterfaces { get; } = new List<InterfaceDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
      if (syntaxNode is InterfaceDeclarationSyntax ids && ids.AttributeLists.Count > 0)
      {
        CandidateInterfaces.Add(ids);
      }
    }
  }

}
