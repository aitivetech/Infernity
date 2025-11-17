using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infernity.Tools.SourceGenerators.Syntax;

internal sealed record ParsedAttribute(string Name, IReadOnlyList<Optional<object?>> Arguments)
{
    internal bool Matches(string attributeName) => Name == attributeName || Name == attributeName + "Attribute";
}

internal static class AttributeDataExtensions
{
    internal static bool HasAttribute(this SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
    {
        return attributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == attributeName ||
                                                               a.Name.ToString() == attributeName + "Attribute"));
    }

    internal static ParsedAttribute? GetAttribute(this SyntaxList<AttributeListSyntax> attributeLists,
                                                  SemanticModel                        semanticModel,
                                                  string                               attributeName)
    {
        foreach (var attribute in GetAttributes(attributeLists, semanticModel))
        {
            if (attribute.Matches(attributeName))
            {
                return attribute;
            }
        }
        
        return null;
    }

    internal static IEnumerable<ParsedAttribute> GetAttributes(this SyntaxList<AttributeListSyntax> attributeLists,
                                                               SemanticModel                        semanticModel)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                IReadOnlyList<Optional<object?>> argumentValues = new List<Optional<object?>>();

                if (attribute.ArgumentList != null)
                {
                    argumentValues = attribute.ArgumentList.Arguments
                                              .Select(argument => semanticModel.GetConstantValue(argument.Expression))
                                              .ToList();
                }

                yield return new ParsedAttribute(attribute.Name.ToString(), argumentValues);
            }
        }
    }

    internal static bool Is<TAttribute>(this AttributeData attributeData)
        where TAttribute : System.Attribute
    {
        return Is(attributeData, typeof(TAttribute).Name);
    }

    internal static bool Is(this AttributeData attributeData, string name)
    {
        return attributeData.AttributeClass != null && attributeData.AttributeClass.Name == name;
    }

    internal static T Read<T>(this AttributeData attributeData) where T : Attribute
    {
        var attribute = attributeData is { AttributeConstructor: not null, ConstructorArguments.Length: > 0 }
            ? (T)Activator.CreateInstance(typeof(T),
                                          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                          null, attributeData.GetActualConstructorParams().ToArray(),
                                          null)
            : (T)Activator.CreateInstance(typeof(T),
                                          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                          null, [],
                                          null);

        // Set named parameters
        foreach (var p in attributeData.NamedArguments)
        {
            var property = typeof(T).GetProperty(p.Key);

            if (property != null)
            {
                property.SetValue(attribute, p.Value.Value);
            }
        }

        return attribute;
    }

    public static IEnumerable<object?> GetActualConstructorParams(this AttributeData attributeData)
    {
        foreach (var arg in attributeData.ConstructorArguments)
        {
            if (arg.Kind == TypedConstantKind.Array)
            {
                // Assume they are strings, but the array that we get from this
                // should actually be of type of the objects within it, be it strings or ints
                foreach (var arrayArg in arg.Values.Select(a => a.Value?.ToString()))
                {
                    if (arrayArg != null)
                    {
                        yield return arrayArg;
                    }
                }
            }
            else
            {
                yield return arg.Value;
            }
        }
    }
}