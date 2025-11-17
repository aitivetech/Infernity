using System.Text;

namespace Infernity.Tools.SourceGenerators.Output;

public static class StringBuilderExtensions
{
    public static unsafe StringBuilder AppendSpan(this StringBuilder builder, ReadOnlySpan<char> span)
    {
        fixed (char* ptr = span)
        {
            builder.Append(ptr, span.Length);
        }

        return builder;
    }
}