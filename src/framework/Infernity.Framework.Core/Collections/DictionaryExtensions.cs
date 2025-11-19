using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Collections;

public static class DictionaryExtensions
{
    extension<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> sources) where TKey : notnull
    {
        public bool TryGetValue(TKey                                       key,
            [MaybeNullWhen(false)] out TValue          value)
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
    }

    extension<TKey, TValue>(IDictionary<TKey, TValue> source)
    {
        public TValue GetOrAdd(TKey                           key,
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

        public IDictionary<TKey, TValue> AddAll(IReadOnlyDictionary<TKey, TValue> source1,
            bool                              set)
        {
            foreach (var entry in source1)
            {
                if (set)
                {
                    source[entry.Key] = entry.Value;
                }
                else
                {
                    source.Add(entry.Key, entry.Value);
                }
            }

            return source;
        }
    }

    extension<TKey, TValue>(IDictionary<TKey, TValue> source) where TKey : notnull
    {
        public IDictionary<TKey, TValue> Clone()
        {
            return source.ToDictionary(k => k.Key, kv => kv.Value);
        }
    }

    extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source) where TKey : notnull
    {
        public Dictionary<TKey, TValue> Clone()
        {
            return source.ToDictionary(k => k.Key, kv => kv.Value);
        }
    }

    extension(object value)
    {
        public IReadOnlyDictionary<string, object?> PropertiesToDictionary()
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
    }

    extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> input)
    {
        public Optional<TValue> GetOptional(TKey key)
        {
            if (input.TryGetValue(key, out var value))
            {
                return value;
            }

            return Optional.None<TValue>();
        }
    }

    extension<TKey, TValue>(IEnumerable<IReadOnlyDictionary<TKey, TValue>> sources) where TKey : notnull
    {
        public IReadOnlyDictionary<TKey, TValue> Merge()
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
    }

    extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source) where TKey : notnull
    {
        public IReadOnlyDictionary<TKey, TValue> Except(IEnumerable<TKey> keys)
        {
            var result = new Dictionary<TKey, TValue>(source);

            foreach (var key in keys)
            {
                result.Remove(key);
            }

            return result;
        }

        public IReadOnlyDictionary<TKey, TValue> Except(params TKey[] keys)
        {
            return source.Except((IEnumerable<TKey>)keys);
        }
    }

    extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source)
    {
        public string ToTableString(Func<TKey, int, string>?               keyFormatter   = null,
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
    }
    
    extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> source) where TKey : notnull
    {
        public bool IsEqual(IReadOnlyDictionary<TKey, TValue> other,
            IEqualityComparer<TValue>? valueComparer = null)
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
}