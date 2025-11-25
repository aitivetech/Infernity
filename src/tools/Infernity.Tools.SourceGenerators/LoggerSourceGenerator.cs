using Infernity.Tools.SourceGenerators.Output;
using Infernity.Tools.SourceGenerators.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infernity.Tools.SourceGenerators;

[Generator]
public sealed class LoggerSourceGenerator : IIncrementalGenerator 
{
     private const string AttributeSourceCode = """
                                               namespace Infernity.GeneratedCode;

                                               #if !TEST_PROJECT
                                               [System.AttributeUsage(System.AttributeTargets.Class)]
                                               internal sealed class AddLoggerAttribute : System.Attribute
                                                    
                                               {
                                                   internal AddLoggerAttribute() 
                                                   { 
                                                   }
                                               }
                                               #endif
                                               """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(sourceContext =>
        {
            sourceContext.AddSource("AddLoggerAttribute.g.cs", AttributeSourceCode);
        });

        var items = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Infernity.GeneratedCode.AddLoggerAttribute",
                (node, _) => node is ClassDeclarationSyntax,
                Generate)
            .Where(o => o != null);

        context.RegisterSourceOutput(items,
            (productionContext, sourceFile) => { sourceFile?.AddToOutput(productionContext); });
    }
    
    private SourceFile? Generate(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var writer = new SourceWriter();

        try
        {
            var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;
            var classSymbol = (INamedTypeSymbol)context.TargetSymbol;

            var namespaceName = classDeclaration.GetNamespaceName();
            var visibility = classSymbol.DeclaredAccessibility;
            var className = classSymbol.ToDisplayString(new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameOnly,
                SymbolDisplayGenericsOptions.IncludeTypeParameters));

            writer.WriteLine("#nullable enable");
            
            writer.WriteLine("using System;");
            writer.WriteLine("using Microsoft.Extensions.Logging;");
            writer.WriteLine("using Infernity.Framework.Core.Patterns;");

            writer.WriteEmptyLines(1);
          
            if (namespaceName != null)
            {
                writer.WriteLine($"namespace {namespaceName};");
            }

            writer.WriteEmptyLines(1);
            
            writer.WriteLine($"{visibility.ToString().ToLowerInvariant()} partial class {className}");
            writer.OpenBlock();
            
            var staticPrefix = classSymbol.IsStatic ? "static " : string.Empty;
            
            writer.WriteLine($@"private {staticPrefix}readonly Lazy<ILogger<{className}>> __loggerHolder = new(() => GlobalsRegistry.Resolve<ILoggerFactory>().CreateLogger<{className}>());");
            writer.WriteLine($"private {staticPrefix}ILogger<{className}> Logger => __loggerHolder.Value;");
            
            writer.CloseBlock();

            writer.WriteEmptyLines(2);
            writer.WriteLine("#nullable disable");
           
        }
        catch (Exception ex)
        {
            writer.WriteLine(ex.ToString());
        }

        return writer.ToSourceFile(
            context.TargetSymbol.Name + ".g.cs");
    }
}