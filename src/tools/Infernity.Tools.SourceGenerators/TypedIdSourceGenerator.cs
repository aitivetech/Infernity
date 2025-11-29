using Infernity.Tools.SourceGenerators.Output;
using Infernity.Tools.SourceGenerators.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infernity.Tools.SourceGenerators;

[Generator]
public class TypedIdSourceGenerator : IIncrementalGenerator
{
    private const string AttributeSourceCode = """
                                               namespace Infernity.GeneratedCode;

                                               #if !TEST_PROJECT
                                               [System.AttributeUsage(System.AttributeTargets.Struct)]
                                               internal sealed class TypedIdAttribute : System.Attribute
                                               {

                                               }
                                               #endif
                                               """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(sourceContext =>
        {
            sourceContext.AddSource("TypedIdAttribute.g.cs",
                AttributeSourceCode);
        });

        var items = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Infernity.GeneratedCode.TypedIdAttribute",
                (node,
                        token) =>
                    node is RecordDeclarationSyntax
                    && node.IsKind(SyntaxKind.RecordStructDeclaration),
                Generate
            )
            .Where(o => o != null);

        context.RegisterSourceOutput(
            items,
            (productionContext,
                sourceFile) =>
            {
                sourceFile?.AddToOutput(productionContext);
            }
        );
    }

    private SourceFile? Generate(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var writer = new SourceWriter();

        SourceFile? Return()
        {
            return writer?.ToSourceFile(context.TargetSymbol.Name + ".g.cs");
        }

        var classDeclaration = (RecordDeclarationSyntax)context.TargetNode;
        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;

        var namespaceName = classDeclaration.GetNamespaceName();
        var visibility = classSymbol.DeclaredAccessibility;
        var className = classSymbol.Name;

        if (classDeclaration.ParameterList == null)
        {
            writer.WriteLine("No parameters specified");
            return Return();
        }

        var properties = classDeclaration
            .ParameterList.Parameters.Select(p =>
                (
                    p.Identifier.Text,
                    p.Type != null ? context.SemanticModel.GetTypeInfo(p.Type).Type : null
                )
            )
            .ToList();

        if (properties.Count != 1)
        {
            writer.WriteLine($"Only one property supported, found: {properties.Count}");
            return writer.ToSourceFile(context.TargetSymbol.Name + ".g.cs");
        }

        var valueProperty = properties.First();

        if (valueProperty.Item2 == null)
        {
            writer.WriteLine("Unable to resolve value type");
        }

        var valueType = valueProperty.Item2!.GetReferenceTypeName();
        var valueName = valueProperty.Text;

        var isSha256 =
            valueName == "Value" && valueType == "Infernity.Framework.Security.Hashing.Sha256Value";

        var isSha1 =
            valueName == "Value" && valueType == "Infernity.Framework.Security.Hashing.ShaValue";

        var isHash = isSha256 || isSha1;
        var hashType = isSha256 ? "Sha256Value" : "ShaValue";

        var isGuid = valueType == "System.Guid";

        var hashBase = isHash ? $", IHashId<{className},{hashType}>" : string.Empty;

        writer.WriteLine("#nullable enable");
        writer.WriteLine("using System;");
        writer.WriteLine("using System.ComponentModel;");
        writer.WriteLine("using System.Numerics;");
        writer.WriteLine("using Infernity.Framework.Json.Converters;");
        writer.WriteLine("using Infernity.Framework.Core.Reflection;");

        writer.WriteEmptyLines(1);

        if (isHash)
        {
            writer.WriteLine("using Infernity.Framework.Security.Hashing;");
            writer.WriteEmptyLines(1);
        }

        if (namespaceName != null)
        {
            writer.WriteLine($"namespace {namespaceName};");
        }

        writer.WriteEmptyLines(1);

        writer.WriteLine($"public sealed class {className}TypeConverter : ParsableTypeConverter<{className}>");
        writer.OpenBlock();
        writer.CloseBlock();

        writer.WriteEmptyLines(2);

        writer.WriteLine($"[TypeConverter(typeof({className}TypeConverter))]");
        writer.WriteLine(
            $"{visibility.ToString().ToLowerInvariant()} readonly partial record struct {className} : IParsable<{className}>,IComparable<{className}>{hashBase},IEqualityOperators<{className},{className},bool>"
        );
        writer.OpenBlock();

        if (isHash)
        {
            writer.WriteLine(
                $"public static {className} Invalid {{ get; }} = new({hashType}.Invalid);"
            );
            writer.WriteEmptyLines(1);

            writer.WriteLine(
                $"public static {className} FromBytes(ReadOnlySpan<byte> bytes) => new({hashType}.FromBytes(bytes));");
            writer.WriteEmptyLines(1);

            writer.WriteLine("public bool IsValid => !Equals(Invalid);");
            writer.WriteLine("public bool IsInvalid => Equals(Invalid);");

            writer.WriteLine($"public byte[] ToArray() => {valueName}.ToArray();");

            writer.WriteEmptyLines(1);
        }

        AddImplicitConversions(writer,
            className,
            valueName,
            valueType);

        AddComparable(writer,
            className,
            valueName,
            valueType);
        writer.WriteEmptyLines(1);

        AddToString(writer,
            className,
            valueName,
            valueType);
        writer.WriteEmptyLines(1);

        AddParsing(writer,
            className,
            valueName,
            valueType);

        writer.CloseBlock();

        writer.WriteEmptyLines(2);

        writer.WriteLine(
            $"public sealed class {className}JsonConverter : StringJsonConverter<{className}>"
        );
        writer.OpenBlock();
        writer.CloseBlock();

        writer.WriteEmptyLines(2);

        writer.WriteLine("#nullable disable");

        return Return();
    }

    private void AddImplicitConversions(
        SourceWriter sourceWriter,
        string className,
        string valueName,
        string valueType
    )
    {
        // String
        if (valueType != "string")
        {
            sourceWriter.WriteLine(
                $"public static implicit operator string({className} value)\n        => value.ToString();"
            );

            sourceWriter.WriteEmptyLines(1);

            sourceWriter.WriteLine(
                $"public static implicit operator {className}(string value)\n        => Parse(value, null);"
            );

            sourceWriter.WriteEmptyLines(1);
        }

        sourceWriter.WriteLine(
            $"public static implicit operator {valueType}({className} value)\n        => value.{valueName};"
        );

        sourceWriter.WriteEmptyLines(1);

        sourceWriter.WriteLine(
            $"public static implicit operator {className}({valueType} value)\n        => new(value);"
        );

        sourceWriter.WriteEmptyLines(1);
    }

    private void AddToString(
        SourceWriter sourceWriter,
        string className,
        string valueName,
        string valueType
    )
    {
        sourceWriter.WriteLine("public override string ToString()");
        sourceWriter.OpenBlock();

        if (valueType == "string")
        {
            sourceWriter.WriteLine($"return {valueName};");
        }
        else
        {
            sourceWriter.WriteLine(
                $"return {valueName}.ToString() ?? throw new InvalidOperationException();"
            );
        }

        sourceWriter.CloseBlock();
    }

    private void AddParsing(
        SourceWriter sourceWriter,
        string className,
        string valueName,
        string valueType
    )
    {
        sourceWriter.WriteLine(
            $"public static {className} Parse(string s, IFormatProvider? provider)"
        );
        sourceWriter.OpenBlock();
        sourceWriter.WriteLine("return TryParse(s,provider,out var result) ? result : ");
        sourceWriter.WriteLine(
            $"       throw new FormatException($\"Could not parse {{s}} into {{typeof({className}).Name}}\");"
        );
        sourceWriter.CloseBlock();

        sourceWriter.WriteEmptyLines(1);

        sourceWriter.WriteLine(
            $"public static bool TryParse(string? s, IFormatProvider? provider, out {className} result)"
        );
        sourceWriter.OpenBlock();

        if (valueType == "string")
        {
            sourceWriter.WriteLine("if (!string.IsNullOrWhiteSpace(s))");
            sourceWriter.OpenBlock();
            sourceWriter.WriteLine("result = new(s.Trim());");
            sourceWriter.WriteLine("return true;");
            sourceWriter.CloseBlock();
        }
        else
        {
            sourceWriter.WriteLine($"if ({valueType}.TryParse(s,provider,out var value))");
            sourceWriter.OpenBlock();
            sourceWriter.WriteLine("result = new(value);");
            sourceWriter.WriteLine("return true;");
            sourceWriter.CloseBlock();
        }

        sourceWriter.WriteLine("result = new();");
        sourceWriter.WriteLine("return false;");

        sourceWriter.CloseBlock();
    }

    private void AddComparable(
        SourceWriter sourceWriter,
        string className,
        string valueName,
        string typeName
    )
    {
        sourceWriter.WriteLine($"public int CompareTo({className} other)");
        sourceWriter.OpenBlock();
        sourceWriter.WriteLine($"return {valueName}.CompareTo(other.{valueName});");
        sourceWriter.CloseBlock();
    }
}