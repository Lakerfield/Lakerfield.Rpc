﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    // Check if the project references "Lakerfield.Rpc.Client"
    var hasClientDependencyCheck = context.CompilationProvider
      .Select((compilation, _) =>
      {
        foreach (var reference in compilation.ReferencedAssemblyNames)
          if (reference.Name == "Lakerfield.Rpc.Client")
            return true;
        return false;
      });

    // Check if the project references "Lakerfield.Rpc.Server"
    var hasServerDependencyCheck = context.CompilationProvider
      .Select((compilation, _) =>
      {
        foreach (var reference in compilation.ReferencedAssemblyNames)
          if (reference.Name == "Lakerfield.Rpc.Server")
            return true;
        return false;
      });

    context.RegisterPostInitializationOutput(static postInitializationContext => {
      postInitializationContext.AddSource("RpcServiceAttribute.g.cs", SourceText.From($$"""
using System;

namespace Lakerfield.Rpc
{
  internal sealed class RpcServiceAttribute : Attribute
  {
  }
}
""", Encoding.UTF8));
    });


    // Find all interfaces with RpcServiceAttribute
    var interfacesWithAttribute = context.SyntaxProvider
      .CreateSyntaxProvider(
        predicate: (s, _) => s is InterfaceDeclarationSyntax,
        transform: (ctx, _) => GetSemanticTargetForGeneration(ctx))
      .Where(m => m is not null)
      .Select((symbol, _) => (INamedTypeSymbol)symbol!)
      .Collect();

    // // Register the source generator to generate the implementation class
    // context.RegisterSourceOutput(interfacesWithAttribute, (spc, symbols) => Execute(spc, symbols));

    // Combine the dependency check with the source generator logic
    var combined = interfacesWithAttribute.Combine(hasServerDependencyCheck).Combine(hasClientDependencyCheck);

    // Register the source generator to generate the implementation class only if the dependency is present
    context.RegisterSourceOutput(combined, (spc, tuple) =>
    {
      var ((symbols, hasServer), hasClient) = tuple;
      foreach (var symbol in symbols.Distinct(SymbolEqualityComparer.Default))
      {
        if (symbol is not null)
        {
          GenerateImplementation2(spc, (INamedTypeSymbol)symbol, hasServer, hasClient);
        }
      }
    });

  }

  private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
  {
    var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;
    var interfaceSymbol = context.SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax);

    if (interfaceSymbol is not null)
    {
      var attributes = interfaceSymbol.GetAttributes();
      foreach (var attribute in attributes)
      {
        if (attribute.AttributeClass?.Name == "RpcServiceAttribute")
        {
          return interfaceSymbol;
        }
      }
    }

    return null;
  }

  private void GenerateImplementation2(SourceProductionContext context, INamedTypeSymbol interfaceSymbol, bool hasServer, bool hasClient)
  {
    var interfaceName = interfaceSymbol.Name;
    var namespaceName = interfaceSymbol.ContainingNamespace.ToDisplayString();
    var className = $"{interfaceName.TrimStart('I')}Implementation";
    var sourceBuilder = new StringBuilder($$"""
        using System;

        namespace {{namespaceName}}
        {
        // server {{hasServer}} client {{hasClient}}
          public class {{className}} : {{interfaceName}}
          {

        """);

    // Implement each method from the interface
    //foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
    foreach (var member in GetAllMembersIncludingInherited(interfaceSymbol).OfType<IMethodSymbol>())
    {
      var methodName = member.Name;
      var returnType = member.ReturnType.ToDisplayString();
      var parameters = string.Join(", ", member.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));

      sourceBuilder.Append($$"""
            public {{returnType}} {{methodName}}({{parameters}})
            {
              throw new NotImplementedException();//
            }


        """);
      //, CancellationToken cancellationToken = default
    }

    sourceBuilder.Append("""
        }
      }

      """);

    // Add the generated source
    context.AddSource($"{className.TrimStart('I')}.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
  }

  public static IEnumerable<ISymbol> GetAllMembersIncludingInherited(INamedTypeSymbol interfaceSymbol)
  {
    // Get all members of the current interface
    var members = new List<ISymbol>(interfaceSymbol.GetMembers());

    // Get all inherited interfaces
    foreach (var inheritedInterface in interfaceSymbol.AllInterfaces)
    {
      // Add members of each inherited interface
      members.AddRange(inheritedInterface.GetMembers());
    }

    return members;
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