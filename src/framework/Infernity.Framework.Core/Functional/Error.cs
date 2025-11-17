using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

using Infernity.Framework.Core.Reflection;

namespace Infernity.Framework.Core.Functional;

public interface IErrorFactory<out T>
{
    static abstract T CreateInstance(string id, int statusCode, string message);
}

public sealed record ErrorPayload(string Id, string Message,int StatusCode);

public abstract class ErrorBase
{
    protected ErrorBase(string id, int statusCode, string message)
    {
        Id = id;
        StatusCode = statusCode;
        Message = message;
    }

    public string Id { get; }
    
    public int StatusCode { get; }
    
    public string Message { get; }

    public ErrorPayload Payload => new(Id, Message,StatusCode);
    
    public bool IsEquivalent(ErrorBase other)
    {
        return Id == other.Id;
    }

    public override string ToString()
    {
        return $"{Id}: {Message}";
    }

    public static IReadOnlyDictionary<string, ErrorBase> GetDeclaredErrors(Type concreteErrorType)
    {
        static KeyValuePair<string, ErrorBase> ReadKeyValuePair(object pair)
        {
            // The reflection access is cached and code generated in NET 7.0.
            var key = (string?)pair.GetType().GetProperty("Key")?.GetValue(pair);
            var value = (ErrorBase?)pair.GetType().GetProperty("Value")?.GetValue(pair);

            if (key == null || value == null)
            {
                throw new InvalidOperationException("Invalid key value pairs");
            }

            return new(key, value);
        }
        
        if (!typeof(ErrorBase).IsAssignableFrom(concreteErrorType) || concreteErrorType.IsAbstract)
        {
            throw new ArgumentException("Not a concrete error type", nameof(concreteErrorType));
        }

        var declaredProperty = typeof(ErrorBase<>).MakeGenericType(concreteErrorType).GetProperty("Declared", 
            BindingFlags.Public | BindingFlags.Static);

        if (declaredProperty != null && declaredProperty.CanRead)
        {
            var declaredErrorsUnTyped = declaredProperty.GetValue(null);

            if (declaredErrorsUnTyped != null)
            {
                var result = new Dictionary<string, ErrorBase>();
                
                var errorEnumerable = (IEnumerable)declaredErrorsUnTyped;

                foreach (var entry in errorEnumerable)
                {
                    var kv = ReadKeyValuePair(entry);
                    result.Add(kv.Key,kv.Value);
                }

                return result;
            }
        }

        throw new ArgumentException("Not a valid error type, Declared property not found",
            nameof(concreteErrorType));
    }
}

public abstract class ErrorBase<T> : ErrorBase
    where T : ErrorBase<T>,IErrorFactory<T>
{
    private static readonly Dictionary<string, T> _declaredErrorsById = new();
    private static bool _wasInitialized = false;
    
    protected ErrorBase(string id, int statusCode, string message) 
        : base(id, statusCode, message)
    {
    }

    public static IReadOnlyDictionary<string, T> Declared
    {
        get
        {
            EnforceStaticConstructorExecution();
            return _declaredErrorsById;
        }
    }

    public static IEnumerable<T> FromStatusCode(int statusCode)
    {
        return Declared.Values.Where(e => e.StatusCode == statusCode);
    }

    public static T FromPayload(ErrorPayload payload)
    {
        return Create(payload.Id,payload.StatusCode, payload.Message);
    }
    
    public TU Convert<TU>()
        where TU: ErrorBase<TU>, IErrorFactory<TU>
    {
        if (TryConvertTo<TU>(out var result))
        {
            return result;
        }

        throw new InvalidCastException(
            $"Cannot convert error {typeof(T).Name}.{Id} to {typeof(TU).Name}.{Id}, undefined");
    }

    public bool TryConvertTo<TU>([MaybeNullWhen(false)]out TU result)
        where TU : ErrorBase<TU>, IErrorFactory<TU>
    {
        if (ErrorBase<TU>.Declared.TryGetValue(Id, out var resultError))
        {
            result = resultError;
            return true;
        }

        result = default;
        return false;
    }
    
    public T WithMessage(string message)
    {
        return Create(Id, StatusCode, message);
    }

    public T WithExtendedMessage(string message, string separator = ": ")
    {
        return WithMessage(this.Message + separator + message);
    }

    public T WithStatusCode(int statusCode)
    {
        return Create(Id,StatusCode,Message);
    }
    
    protected static T Declare(string message, int statusCode = 400, [CallerMemberName] string? id = null)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (_declaredErrorsById.TryGetValue(id, out var existingError))
        {
            return existingError;
        }
        
        
        var newError = Create(id, statusCode, message);
        _declaredErrorsById.Add(id,newError);
        
        return newError;
    }
    
    protected static T ParseCore(string s, IFormatProvider? provider)
    {
        if (TryParseCore(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Unable to parse {s} into {typeof(T)}");
    }

    protected static bool TryParseCore(string? s, IFormatProvider? provider, 
        [MaybeNullWhen(false)]out T result)
    {
        EnforceStaticConstructorExecution();
        
        if (s == null)
        {
            result = null;
            return false;
        }
        
        if (Declared.TryGetValue(s, out var error))
        {
            result = error;
            return true;
        }

        result = null;
        return false;
    }

    private static T Create(string id, int statusCode, string message)
    {
        return T.CreateInstance(id, statusCode, message);
    }

    private static void EnforceStaticConstructorExecution()
    {
        if (!_wasInitialized)
        {
            typeof(T).EnsureStaticConstructorExecution();
            _wasInitialized = true;
        }
    }
}
