using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Infernity.Tools.SourceGenerators.Syntax;

internal static class TypeDeclarationSyntaxExtensions
{
    internal static bool HasDeclaredMethod(this ClassDeclarationSyntax classDeclarationSyntax,string methodName)
    {
        return classDeclarationSyntax.Members
            .OfType<MethodDeclarationSyntax>()
            .Any(m => m.Identifier.Text == methodName);
    } 
    
    internal static string GetUsingDeclarations(this BaseTypeDeclarationSyntax syntax)
    {
        var compilationUnit = syntax.SyntaxTree.GetRoot() as CompilationUnitSyntax;

        if (compilationUnit == null)
        {
            return string.Empty;
        }

        // Extract all using directives
        var usingDirectives = compilationUnit.Usings;

        // Build the output text
        var output = new StringBuilder();

        foreach (var usingDirective in usingDirectives)
        {
            output.AppendLine(usingDirective.ToFullString().Trim());
        }

        return output.ToString();
    }
    
    internal static bool IsSealed(this BaseTypeDeclarationSyntax syntax)
    {
        return syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword));
    }

    internal static bool IsAbstract(this BaseTypeDeclarationSyntax syntax)
    {
        return syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
    }

    internal static bool IsDefinedInTestProject(this BaseTypeDeclarationSyntax syntax)
    {
        var namespaceName = syntax.GetNamespaceName();

        if (namespaceName != null)
        {
            var parts = namespaceName.Split('.');

            return parts.Any(p => p == "Tests");
        }

        return false;
    }

    internal static string? GetNamespaceName(this BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        var nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        var potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return string.IsNullOrEmpty(nameSpace) ? null : nameSpace;
    }
}