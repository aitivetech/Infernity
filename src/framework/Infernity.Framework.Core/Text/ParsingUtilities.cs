using System.Diagnostics.CodeAnalysis;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Text;

public static class ParsingUtilities
{
    public static T ParseCore<T>(string? s,
        IFormatProvider? provider)
        where T : IParsable<T>
    {
        if (s == null)
        {
            throw new FormatException($"Could not parse {nameof(s)} from null value");
        }
        
        if (!T.TryParse(s,
                provider,
                out T? result))
        {
            throw new FormatException($"Could not parse {nameof(T)}: {s}");
        }
        
        return result;
    }
    
    public static bool TryParseConcatenatedValues<T1, T2>(string? input,
        string separator,
        [NotNullWhen(true)] out T1? v1,
        [NotNullWhen(true)] out T2? v2)
        where T1 : IParsable<T1>
        where T2 : IParsable<T2>
    {
        return TryParseConcatenatedValues<T1, T2>(input,
            separator,
            v => ParsableExtensions.TryParseOptional<T2>(v),
            out v1,
            out v2);
    }

    public static bool TryParseConcatenatedValues<T1, T2>(string? input,
        string separator,
        Func<string, Optional<T2>> t2Parser,
        [NotNullWhen(true)] out T1? v1,
        [NotNullWhen(true)] out T2? v2)
        where T1 : IParsable<T1>
    {
        return TryParseConcatenatedValues<T1, T2>(input,
            separator,
            v => ParsableExtensions.TryParseOptional<T1>(v),
            t2Parser,
            out v1,
            out v2);
    }

    public static bool TryParseConcatenatedValues<T1, T2>(string? input,
        string separator,
        Func<string, Optional<T1>> t1Parser,
        Func<string, Optional<T2>> t2Parser,
        [NotNullWhen(true)] out T1? v1,
        [NotNullWhen(true)] out T2? v2)
    {
        v1 = default(T1);
        v2 = default(T2);
        
        if (input == null)
        {
            return false;
        }
        
        var firstPosition = input.IndexOf(separator, StringComparison.Ordinal);

        if (firstPosition < 0 || firstPosition + separator.Length + 1 >= input.Length)
        {
            return false;
        }
        
        var p1 = input.Substring(0, firstPosition);
        var p2 = input.Substring(firstPosition + separator.Length);

        var v1Optional = t1Parser(p1);
        var v2Optional = t2Parser(p2);

        if (!v1Optional.HasValue || !v2Optional.HasValue)
        {
            return false;
        }

        v1 = v1Optional.Value!;
        v2 = v2Optional.Value!;

        return true;
    }
}