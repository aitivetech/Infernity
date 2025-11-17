using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Collections;

public static class DictionaryExtensions
{
    public static bool TryGetValue<TKey, TValue>(this IEnumerable<Dictionary<TKey, TValue>> sources,
                                                 TKey                                       key,
                                                 [MaybeNullWhen(false)] out TValue          value)
        where TKey : notnull
    {
        foreach (var source in sources)
        {
            if (source.TryGetValue(key, out value))
            {
                return true;
            }
        }

        value = default;
        return false;
    }

    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> source,
        TKey                           key,
        Func<TKey, TValue>             factory)
    {
        if (source.TryGetValue(key, out var result))
        {
            return result;
        }

        result = factory.Invoke(key);
        source.Add(key, result);
        return result;
    }

    public static IDictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue> source)
        where TKey : notnull
    {
        return source.ToDictionary(k => k.Key, kv => kv.Value);
    }

    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source)
        where TKey : notnull
    {
        return source.ToDictionary(k => k.Key, kv => kv.Value);
    }

    public static IDictionary<TKey, TValue> AddAll<TKey, TValue>(
        this IDictionary<TKey, TValue>    target,
        IReadOnlyDictionary<TKey, TValue> source,
        bool                              set)
    {
        foreach (var entry in source)
        {
            if (set)
            {
                target[entry.Key] = entry.Value;
            }
            else
            {
                target.Add(entry.Key, entry.Value);
            }
        }

        return target;
    }

    public static IReadOnlyDictionary<string, object?> PropertiesToDictionary(this object value)
    {
        if (value is IReadOnlyDictionary<string, object?> dict)
        {
            return dict;
        }

        var result = new Dictionary<string, object?>();

        foreach (var propertyInfo in value
                                     .GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (propertyInfo.CanRead)
            {
                var propertyValue = propertyInfo.GetValue(value);
                result.Add(propertyInfo.Name, propertyValue);
            }
        }

        return result;
    }

    public static Optional<TValue> GetOptional<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> input, TKey key)
    {
        if (input.TryGetValue(key, out var value))
        {
            return value;
        }

        return Optional.None<TValue>();
    }

    public static IReadOnlyDictionary<TKey, TValue> Merge<TKey, TValue>(
        this IEnumerable<IReadOnlyDictionary<TKey, TValue>> sources)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();

        foreach (var source in sources)
        {
            foreach (var entry in source)
            {
                result.TryAdd(entry.Key, entry.Value);
            }
        }

        return result;
    }

    public static IReadOnlyDictionary<TKey, TValue> Except<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> source, IEnumerable<TKey> keys)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(source);

        foreach (var key in keys)
        {
            result.Remove(key);
        }

        return result;
    }

    public static IReadOnlyDictionary<TKey, TValue> Except<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> source, params TKey[] keys)
        where TKey : notnull
    {
        return source.Except((IEnumerable<TKey>)keys);
    }

    public static string ToTableString<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> source,
        Func<TKey, int, string>?               keyFormatter   = null,
        Func<TValue, int, string>?             valueFormatter = null,
        string                                 seperator      = " = ")
    {
        static string DefaultKeyFormatter(TKey key, int index)
        {
            return $"({index}): {key}";
        }

        static string DefaultValueFormatter(TValue value, int index)
        {
            return $"{value}";
        }

        var actualKeyFormatter = keyFormatter ?? DefaultKeyFormatter;
        var actualValueFormatter = valueFormatter ?? DefaultValueFormatter;

        var builder = new StringBuilder();

        foreach (var entry in source.Select((entry, index) => (entry, index)))
        {
            var line = $"{actualKeyFormatter(entry.entry.Key,entry.index)}{seperator}{actualValueFormatter(entry.entry.Value, entry.index)}";
            builder.AppendLine(line);
        }
        
        return builder.ToString();
    }
    
    public static bool IsEqual<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> other,
        IEqualityComparer<TValue>? valueComparer = null)
        where TKey : notnull
    {
        if (source.Count != other.Count)
        {
            return false;
        }

        var comparer = valueComparer ?? EqualityComparer<TValue>.Default;

        foreach (var entry in source)
        {
            if (!other.TryGetValue(entry.Key, out var otherValue) || !comparer.Equals(entry.Value, otherValue))
            {
                return false;
            }
        }

        return true;
    }
}