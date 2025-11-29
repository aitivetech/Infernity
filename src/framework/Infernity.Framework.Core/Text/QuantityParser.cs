using System.Globalization;
using System.Text.RegularExpressions;

namespace Infernity.Framework.Core.Text;

/// <summary>
///     Parser for scientific/computing quantities using SI prefixes (K, M, G, T, P, E)
///     Examples: FLOPS, Hz, bytes, scientific measurements
/// </summary>
public static class ScientificQuantityParser
{
    private static readonly Dictionary<string, long> Multipliers = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        // SI prefixes (decimal)
        ["k"] = 1_000L, // kilo
        ["m"] = 1_000_000L, // mega
        ["g"] = 1_000_000_000L, // giga
        ["t"] = 1_000_000_000_000L, // tera
        ["p"] = 1_000_000_000_000_000L, // peta
        ["e"] = 1_000_000_000_000_000_000L, // exa

        // Full names
        ["kilo"] = 1_000L,
        ["mega"] = 1_000_000L,
        ["giga"] = 1_000_000_000L,
        ["tera"] = 1_000_000_000_000L,
        ["peta"] = 1_000_000_000_000_000L,
        ["exa"] = 1_000_000_000_000_000_000L,

        // Binary prefixes (common in computing)
        ["ki"] = 1024L, // kibi
        ["mi"] = 1024L * 1024L, // mebi
        ["gi"] = 1024L * 1024L * 1024L, // gibi
        ["ti"] = 1024L * 1024L * 1024L * 1024L, // tebi

        // Common computing notations
        ["kb"] = 1_000L, // kilobyte (decimal)
        ["mb"] = 1_000_000L, // megabyte (decimal)
        ["gb"] = 1_000_000_000L, // gigabyte (decimal)
        ["tb"] = 1_000_000_000_000L, // terabyte (decimal)
        ["pb"] = 1_000_000_000_000_000L, // petabyte (decimal)
        ["eb"] = 1_000_000_000_000_000_000L, // exabyte (decimal)

        // FLOPS-specific (common usage)
        ["kflops"] = 1_000L,
        ["mflops"] = 1_000_000L,
        ["gflops"] = 1_000_000_000L,
        ["tflops"] = 1_000_000_000_000L,
        ["pflops"] = 1_000_000_000_000_000L,
        ["eflops"] = 1_000_000_000_000_000_000L
    };

    public static long Parse(string input)
    {
        return QuantityParserCore.ParseCore(input,
            Multipliers);
    }

    public static bool TryParse(string input,
        out long result)
    {
        return QuantityParserCore.TryParseCore(input,
            Multipliers,
            out result);
    }

    public static string ToString(long value,
        QuantityFormat format = QuantityFormat.Compact)
    {
        if (value == 0)
        {
            return "0";
        }

        var absValue = System.Math.Abs(value);
        var sign = value < 0 ? "-" : "";

        var multipliers = new[]
        {
            ("E", 1_000_000_000_000_000_000L), ("P", 1_000_000_000_000_000L), ("T", 1_000_000_000_000L),
            ("G", 1_000_000_000L), // Giga for scientific
            ("M", 1_000_000L), ("K", 1_000L)
        };

        return QuantityParserCore.FormatWithMultipliers(absValue,
            sign,
            multipliers,
            format);
    }
}

/// <summary>
///     Parser for model/parameter quantities using common abbreviations (K, M, B, T)
///     Examples: model parameters, dataset sizes, human-readable counts
/// </summary>
public static class ModelQuantityParser
{
    private static readonly Dictionary<string, long> Multipliers = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        // Common model parameter notation
        ["k"] = 1_000L, // thousand
        ["m"] = 1_000_000L, // million
        ["b"] = 1_000_000_000L, // billion
        ["t"] = 1_000_000_000_000L, // trillion

        // Full names
        ["thousand"] = 1_000L,
        ["million"] = 1_000_000L,
        ["billion"] = 1_000_000_000L,
        ["trillion"] = 1_000_000_000_000L,

        // Alternative notations
        ["thou"] = 1_000L,
        ["mil"] = 1_000_000L,
        ["bil"] = 1_000_000_000L,
        ["tril"] = 1_000_000_000_000L
    };

    public static long Parse(string input)
    {
        return QuantityParserCore.ParseCore(input,
            Multipliers);
    }

    public static bool TryParse(string input,
        out long result)
    {
        return QuantityParserCore.TryParseCore(input,
            Multipliers,
            out result);
    }

    public static string ToString(long value,
        QuantityFormat format = QuantityFormat.Compact)
    {
        if (value == 0)
        {
            return "0";
        }

        var absValue = System.Math.Abs(value);
        var sign = value < 0 ? "-" : "";

        var multipliers = new[]
        {
            ("T", 1_000_000_000_000L), // Trillion
            ("B", 1_000_000_000L), // Billion for models
            ("M", 1_000_000L), // Million
            ("K", 1_000L) // Thousand
        };

        return QuantityParserCore.FormatWithMultipliers(absValue,
            sign,
            multipliers,
            format);
    }
}

public enum QuantityFormat
{
    /// <summary>Most compact representation (default)</summary>
    Compact,

    /// <summary>Only use abbreviations for exact multiples</summary>
    Precise,

    /// <summary>Always abbreviate when >= 1000</summary>
    AlwaysAbbreviate
}

// Extension methods for convenience
public static class LongQuantityExtensions
{
    public static string ToScientificString(
        this long value,
        QuantityFormat format = QuantityFormat.Compact
    )
    {
        return ScientificQuantityParser.ToString(value,
            format);
    }

    public static string ToModelString(
        this long value,
        QuantityFormat format = QuantityFormat.Compact
    )
    {
        return ModelQuantityParser.ToString(value,
            format);
    }
}

// Shared implementation details
internal static class QuantityParserCore
{
    private static readonly Regex ParseRegex = new(
        @"^\s*([+-]?\d*\.?\d+)\s*([a-zA-Z]*)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static long ParseCore(string input,
        Dictionary<string, long> multipliers)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or whitespace",
                nameof(input));
        }

        var match = ParseRegex.Match(input.Trim());

        if (!match.Success)
        {
            throw new FormatException($"Invalid format: '{input}'");
        }

        var numberPart = match.Groups[1].Value;
        var suffixPart = match.Groups[2].Value;

        if (
            !double.TryParse(
                numberPart,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var number
            )
        )
        {
            throw new FormatException($"Invalid number format: '{numberPart}'");
        }

        long multiplier = 1;
        if (!string.IsNullOrEmpty(suffixPart))
        {
            if (!multipliers.TryGetValue(suffixPart,
                    out multiplier))
            {
                throw new FormatException($"Unknown quantity suffix: '{suffixPart}'");
            }
        }

        try
        {
            return (long)(number * multiplier);
        }
        catch (OverflowException)
        {
            throw new OverflowException(
                $"Value '{input}' results in overflow when converted to long"
            );
        }
    }

    public static bool TryParseCore(
        string input,
        Dictionary<string, long> multipliers,
        out long result
    )
    {
        result = 0;
        try
        {
            result = ParseCore(input,
                multipliers);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string FormatWithMultipliers(
        long absValue,
        string sign,
        (string suffix, long value)[] multipliers,
        QuantityFormat format
    )
    {
        return format switch
        {
            QuantityFormat.Compact => FormatCompact(absValue,
                sign,
                multipliers),
            QuantityFormat.Precise => FormatPrecise(absValue,
                sign,
                multipliers),
            QuantityFormat.AlwaysAbbreviate => FormatAlwaysAbbreviate(absValue,
                sign,
                multipliers),
            _ => $"{sign}{absValue}"
        };
    }

    private static string FormatCompact(
        long absValue,
        string sign,
        (string suffix, long value)[] multipliers
    )
    {
        foreach (var (suffix, multiplier) in multipliers)
        {
            if (absValue >= multiplier)
            {
                var divided = (double)absValue / multiplier;

                // Use integers when possible for cleaner output
                if (System.Math.Abs(divided - System.Math.Floor(divided)) < float.Epsilon)
                {
                    return $"{sign}{(long)divided}{suffix}";
                }

                // Use minimal decimal places
                if (divided >= 100)
                {
                    return $"{sign}{divided:F0}{suffix}";
                }

                if (divided >= 10)
                {
                    return $"{sign}{divided:F1}{suffix}";
                }

                return $"{sign}{divided:F2}{suffix}";
            }
        }

        return $"{sign}{absValue}";
    }

    private static string FormatPrecise(
        long absValue,
        string sign,
        (string suffix, long value)[] multipliers
    )
    {
        foreach (var (suffix, multiplier) in multipliers)
        {
            if (absValue >= multiplier && absValue % multiplier == 0)
            {
                var divided = absValue / multiplier;
                return $"{sign}{divided}{suffix}";
            }
        }

        return $"{sign}{absValue}";
    }

    private static string FormatAlwaysAbbreviate(
        long absValue,
        string sign,
        (string suffix, long value)[] multipliers
    )
    {
        if (absValue < 1_000)
        {
            return $"{sign}{absValue}";
        }

        return FormatCompact(absValue,
            sign,
            multipliers);
    }
}