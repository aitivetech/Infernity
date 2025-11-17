namespace Infernity.Framework.Core.Functional;

public interface IResult
{
    public bool IsSuccessful { get; }
    public bool HasFailed { get; }
    
    public Type ValueType { get; }
    
    public Type ErrorType { get; }
    
    public object Value { get; }
    
    public object Error { get; }
}

public readonly struct Result<T,TError> : IResult,IAsyncDisposable,IDisposable
{
    private readonly Optional<T> _value;
    private readonly Optional<TError> _error;

    public static implicit operator Result<T, TError>(T value)
    {
        return new(value);
    }

    public static implicit operator Result<T, TError>(TError error)
    {
        return new(error);
    }

    public static bool operator true(in Result<T, TError> x) => x.IsSuccessful;
    public static bool operator false(in Result<T, TError> x) => x.HasFailed;
    public static bool operator !(in Result<T, TError> x) => x.HasFailed;
    
    public Result(T value)
    {
        _value = value;
        _error = Optional<TError>.None;
    }

    public Result(TError error)
    {
        _value = Optional<T>.None;
        _error = error;
    }

    public bool IsSuccessful => _value.HasValue;
    public bool HasFailed => _error.HasValue;
    public Type ValueType => typeof(T);
    public Type ErrorType => typeof(TError);
    object IResult.Value => Value ?? throw new InvalidOperationException();
    object IResult.Error => Error ?? throw new InvalidOperationException();

    public bool HasValue => IsSuccessful;
    public bool HasError => HasFailed;
    
    public T Value => _value.Value;
    public TError Error => _error.Value;

    public Optional<T> ToOptional()
        => _value;

    public Result<T, TMappedError> SelectError<TMappedError>(
        Func<TError, TMappedError> mappingFunction)
    {
        if (HasFailed)
        {
            return mappingFunction.Invoke(Error);
        }

        return Value;
    }
    
    public T Or(T defaultValue)
        => HasValue ? Value : defaultValue;

    public T OrThrow(Func<TError,Exception> exceptionFactory)
    {
        if (HasFailed)
        {
            throw exceptionFactory.Invoke(Error);
        }
        
        return Value;
    }
    
    public Result<TMapped,TError> Select<TMapped>(
        Func<T, TMapped> mappingFunction)
    {
        if (IsSuccessful)
        {
            return mappingFunction.Invoke(Value);
        }

        return Error;
    }
    
    public async ValueTask<Result<TMapped,TError>> Select<TMapped>(
        Func<T, ValueTask<TMapped>> mappingFunction)
    {
        if (IsSuccessful)
        {
            return await mappingFunction.Invoke(Value);
        }

        return Error;
    }
    
    public ValueTask DisposeAsync()
    {
        if (HasValue && Value is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        if (HasValue && Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public override string ToString()
    {
        return (HasValue ? Value?.ToString() : Error?.ToString()) ?? string.Empty;
    }
}

public static class Result
{
    public static bool IsResult(this Type resultType) =>
        resultType.IsConstructedGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<,>);

    public static (Type ValueType, Type ErrorType) GetUnderlyingTypes(Type resultType)
    {
        if (!resultType.IsResult())
        {
            throw new ArgumentException("Type is not a Result<,> type");
        }

        if (!resultType.IsConstructedGenericType)
        {
            throw new ArgumentException("Type is an open generic type definition");
        }

        var arguments = resultType.GetGenericArguments();

        return (arguments[0], arguments[1]);
    }
}