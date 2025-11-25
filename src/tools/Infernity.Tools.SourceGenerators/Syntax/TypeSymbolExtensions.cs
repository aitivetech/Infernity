using Microsoft.CodeAnalysis;

namespace Infernity.Tools.SourceGenerators.Syntax;

internal static class TypeSymbolExtensions
{
    extension(ITypeSymbol typeSymbol)
    {
        internal bool HasConstMember(string constName, bool includeBases = true)
        {
            return typeSymbol.HasMember(constName, symbol => symbol is IFieldSymbol { IsConst: true });
        }

        internal ISymbol? FindConstMember(string               memberName,
            bool                 includeBases = true)
        {
            return FindMember(typeSymbol,memberName,s => s is IFieldSymbol { IsConst: true }, includeBases);
        }

        internal bool HasMember(string               memberName,
            Func<ISymbol, bool>? predicate    = null,
            bool                 includeBases = true)
        {
            return FindMember(typeSymbol, memberName, predicate, includeBases) != null;
        }

        internal ISymbol? FindMember(string               memberName,
            Func<ISymbol, bool>? predicate    = null,
            bool                 includeBases = true)
        {
            var finalPredicate = predicate ?? (_ => true);

            var result = typeSymbol
                .GetMembers()
                .FirstOrDefault(m => m.Name == memberName && finalPredicate(m));

            if (result != null)
            {
                return result;
            }

            if (typeSymbol.BaseType != null && includeBases)
            {
                return FindMember(typeSymbol.BaseType, memberName, predicate, includeBases);
            }

            return null;
        }

        internal string GetReferenceTypeName()
        {
            return typeSymbol.ToDisplayString(new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle
                    .NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
                propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            ));
        }
    }


    internal static bool HasAnyBaseDeclarations(this INamedTypeSymbol typeSymbol)
    {
        return (typeSymbol.BaseType != null && typeSymbol.BaseType.Name != "Object") || typeSymbol.AllInterfaces.Any();
    }

    internal static bool HasNoUserSpecifiedBaseType(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.BaseType == null || typeSymbol.BaseType.Name == "Object";
    }
}