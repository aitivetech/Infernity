namespace Infernity.Framework.Core.Functional;

public record ResultCollection<T,TError>(IReadOnlyCollection<Result<T,TError>> Results)
{
    public bool AllSuccessful => Results.All(r => r.IsSuccessful);
    public bool AnyFailed => Results.Any(r => r.HasFailed);

    public static bool operator true(in ResultCollection<T, TError> x) => x.AllSuccessful;
    public static bool operator false(in ResultCollection<T, TError> x) => x.AnyFailed;
    public static bool operator !(in ResultCollection<T, TError> x) => x.AnyFailed;

    public ResultCollection<TNew, TError> Select<TNew>(Func<T, TNew> selector)
        => new(Results.Select(r => r.Select(selector)).ToList());
    
    public ResultCollection<T,TNewError> SelectError<TNewError>(Func<TError,TNewError> errorSelector)
        => new(Results.Select(r => r.SelectError(errorSelector)).ToList());
}
