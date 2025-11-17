namespace Infernity.Framework.Security.Hashing;

public interface IHashId<THash>
    where THash : struct, IHashValue<THash>
{
    THash Value { get; }
}

public interface IHashId<TId, THash> : IHashId<THash>
    where TId : struct, IHashId<TId, THash>
    where THash : struct, IHashValue<THash>
{
    static abstract TId Invalid { get; }

    static abstract TId FromBytes(ReadOnlySpan<byte> bytes);

    static TId From<TFrom>(TFrom from)
        where TFrom : struct, IHashId<TFrom, THash>
    {
        return TId.FromBytes(from.Value.ToArray());
    }

    TTo To<TTo>()
        where TTo : struct, IHashId<TTo, THash>
    {
        return TTo.FromBytes(Value.ToArray());
    }
}
