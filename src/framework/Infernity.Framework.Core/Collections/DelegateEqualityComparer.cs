namespace Infernity.Framework.Core.Collections;

public sealed class DelegateEqualityComparer<T> : IEqualityComparer<T>
    where T : notnull
{
    private readonly Func<T, T, bool> _equalsFunc;
    private readonly Func<T, int> _hashCodeFunc;

    public DelegateEqualityComparer(Func<T, T, bool> equalsFunc, Func<T, int>? hashCodeFunc = null)
    {
        _equalsFunc = equalsFunc;
        _hashCodeFunc = hashCodeFunc ?? (a => a.GetHashCode());
    }

    public bool Equals(T? x, T? y)
    {
        if (!(x is not  null && y is not null))
        {
            return false;
        }

        return _equalsFunc.Invoke(x, y);
    }

    public int GetHashCode(T obj)
    {
        return _hashCodeFunc.Invoke(obj);
    }
}