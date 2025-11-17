namespace Infernity.Framework.Core.Functional;

public abstract record ResultDictionary<TKey,TValue,TError>(IReadOnlyDictionary<TKey,Result<TValue,TError>> Results)
    where TKey : notnull
{
    public bool AllSuccessful => Results.Values.All(r => r.IsSuccessful);
    public bool AnyFailed => Results.Values.Any(r => r.HasFailed);
    public static bool operator true(in ResultDictionary<TKey,TValue, TError> x) => x.AllSuccessful;
    public static bool operator false(in ResultDictionary<TKey,TValue, TError> x) => x.AnyFailed;
    public static bool operator !(in ResultDictionary<TKey,TValue, TError> x) => x.AnyFailed;
}
