using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Infernity.Tools.SourceGenerators.Output;

public sealed record SourceFile(string Filename, string Content)
{
    public static implicit operator SourceText(SourceFile sourceFile)
    {
        return SourceText.From(sourceFile.Content, Encoding.UTF8);
    }
    
    public void AddToOutput(SourceProductionContext context)
    {
        context.AddSource(Filename,this);
    }
}