using System.Text;

namespace Infernity.Framework.Core.Text;

public static class StringBuilderExtensions
{
    extension(StringBuilder builder)
    {
        public StringBuilder AppendIfNotEqual<T>(T value,
            T comparand,IEqualityComparer<T>? equalityComparer = null)
        {
            var finalComparer = equalityComparer ?? EqualityComparer<T>.Default;

            if (!finalComparer.Equals(value,
                    comparand))
            {
                builder.Append(value);
            }
            
            return builder;
        }
    }
}