using Infernity.Tools.SourceGenerators.Output;

using Infernity.Tools.SourceGenerators.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infernity.Tools.SourceGenerators;

[Generator]
public class ErrorSourceGenerator : IIncrementalGenerator
{
    private const string AttributeSourceCode = """
                                               namespace InfernityServer.GeneratedCode;
                                                #if !TEST_PROJECT
                                               [System.AttributeUsage(System.AttributeTargets.Class)]
                                               internal sealed class ErrorAttribute : System.Attribute
                                               {
                                                   
                                               }
                                               #endif
                                               """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(sourceContext =>
        {
            sourceContext.AddSource("ErrorAttribute.g.cs", AttributeSourceCode);
        });

        var items = context.SyntaxProvider.ForAttributeWithMetadataName(
                "InfernityServer.GeneratedCode.ErrorAttribute",
                (node, token) => node is ClassDeclarationSyntax,
                Generate)
            .Where(o => o != null);

        context.RegisterSourceOutput(items,
            (productionContext, sourceFile) => { sourceFile?.AddToOutput(productionContext); });
    }

    private SourceFile? Generate(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var writer = new SourceWriter();

        var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;
        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;

        var namespaceName = classDeclaration.GetNamespaceName();
        var visibility = classSymbol.DeclaredAccessibility;
        var className = classSymbol.Name;

        var typeParameter = classDeclaration.TypeParameterList?.Parameters.First();
        
        writer.WriteLine("#nullable enable");
        
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Numerics;");
        writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
        writer.WriteLine("using Infernity.Framework.Core.Functional;");
      
        writer.WriteEmptyLines(1);
        
        if (namespaceName != null)
        {
            writer.WriteLine($"namespace {namespaceName};");
        }

        writer.WriteEmptyLines(1);

        var basesToAdd = new List<string>();

        if (classSymbol.HasNoUserSpecifiedBaseType())
        {
            var baseParameter = typeParameter?.Identifier.Text ?? className;   
            basesToAdd.Add($"ErrorBase<{baseParameter}>");
        }

        if (!classDeclaration.IsAbstract())
        {
            basesToAdd.Add($"IErrorFactory<{className}>");
            basesToAdd.Add($"IEquatable<{className}>");
            basesToAdd.Add($"IEqualityOperators<{className},{className},bool>");
            basesToAdd.Add($"IParsable<{className}>");
        }

        var baseText = basesToAdd.Any() ? " : " + string.Join(",", basesToAdd) : string.Empty;

        var typeParameterSyntax = typeParameter != null ? $"<{typeParameter.Identifier.Text}>" : string.Empty;
        
        writer.WriteLine($"{visibility.ToString().ToLowerInvariant()} partial class {className}{typeParameterSyntax}{baseText}");

        if (typeParameter != null)
        {
            var typeParameterName = typeParameter.Identifier.Text;
            writer.WriteLine(
                $" where {typeParameterName} : ErrorBase<{typeParameterName}>,IErrorFactory<{typeParameterName}>");
        }
        
        writer.OpenBlock();
      
        AddConstructor(writer,className,classDeclaration.IsSealed());

        var baseClassType = classSymbol.BaseType;

        if (baseClassType != null && baseClassType.Name != "Object")
        {
            //AddStaticConstructor(writer, className, baseClassType);
        }

        if (!classDeclaration.IsAbstract())
        {
            AddEqualityOperators(writer, className);
            AddErrorFactoryImplementation(writer, className);
            AddParsable(writer, className);
            AddTypedEquals(writer, className);
            AddEquals(writer, className);
            AddGetHashCode(writer, className);
        }

        writer.CloseBlock();

        writer.WriteEmptyLines(1);
        writer.WriteLine("#nullable disable");
        
        return writer.ToSourceFile(
            context.TargetSymbol.Name + ".g.cs");
    }

    private void AddStaticConstructor(SourceWriter sourceWriter, string className,INamedTypeSymbol baseClassType)
    {
        sourceWriter.WriteLine($"static {className}()");
        sourceWriter.OpenBlock();
 
        var property = baseClassType
                       .GetMembers().
                       OfType<IPropertySymbol>()
                       .FirstOrDefault(p => p.GetMethod != null && p.IsStatic);

        if (property != null)
        {
            sourceWriter
                .WriteLine($"_ = {baseClassType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{property.Name};");
        }

        sourceWriter.CloseBlock();
    }

    private void AddConstructor(SourceWriter sourceWriter, string className, bool isSealed)
    {
        var visibility = isSealed ? "private" : "protected";

        sourceWriter.WriteLine($"{visibility} {className}(string id,int statusCode,string message) : base(id,statusCode,message)");
        sourceWriter.OpenBlock();
        sourceWriter.CloseBlock();
    }

    private void AddErrorFactoryImplementation(SourceWriter sourceWriter, string className)
    {
        sourceWriter.WriteLine(
            $"static {className} IErrorFactory<{className}>.CreateInstance(string id, int statusCode, string message)");

        sourceWriter.OpenBlock();

        sourceWriter.WriteLine("return new(id,statusCode,message);");
        
        sourceWriter.CloseBlock();
    }

    private void AddParsable(SourceWriter writer, string className)
    {
        writer.WriteLine($"public static {className} Parse(string s,IFormatProvider? provider)");
        writer.OpenBlock();

        writer.WriteLine("return ParseCore(s,provider);");
        
        writer.CloseBlock();

        writer.WriteEmptyLines(1);

        writer.WriteLine(
            $"public static bool TryParse(string? s,IFormatProvider? provider,[MaybeNullWhen(false)]out {className} result)");

        writer.OpenBlock();

        writer.WriteLine("return TryParseCore(s,provider,out result);");
        
        writer.CloseBlock();
    }

    private void AddTypedEquals(SourceWriter writer, string className)
    {
        writer.WriteLine($"public bool Equals({className}? other)");
        writer.OpenBlock();
        
        writer.WriteLine("if (ReferenceEquals(null, other)) return false;");
        writer.WriteLine("if (ReferenceEquals(this, other)) return true;");
        writer.WriteLine("return Id == other.Id;");
        
        writer.CloseBlock();
    }

    private void AddEquals(SourceWriter writer, string className)
    {
        writer.WriteLine("public override bool Equals(object? obj)");
        writer.OpenBlock();
        writer.WriteLine($" return ReferenceEquals(this, obj) || obj is {className} other && Equals(other);");
        writer.CloseBlock();
    }

    private void AddGetHashCode(SourceWriter writer, string className)
    {
        writer.WriteLine("public override int GetHashCode()");
        writer.OpenBlock();
        writer.WriteLine("return Id.GetHashCode();");
        writer.CloseBlock();
    }

    private void AddEqualityOperators(SourceWriter writer, string className)
    {
        writer.WriteLine($"public static bool operator ==({className}? left, {className}? right)");
        writer.OpenBlock();
        writer.WriteLine("return Equals(left, right);");
        writer.CloseBlock();

        writer.WriteLine($"public static bool operator !=({className}? left, {className}? right)");
        writer.OpenBlock();
        writer.WriteLine("return !Equals(left, right);");
        writer.CloseBlock();
    }
}

