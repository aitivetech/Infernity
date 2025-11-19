using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Reflection;

public interface ITypeResolver<TTypeId>
{
    Optional<Type> Resolve(TTypeId id);
    Optional<TTypeId> Resolve(Type type);
}

public sealed class StaticTypeResolver<TTypeId> : ITypeResolver<TTypeId>
    where TTypeId : notnull
{
    private readonly Dictionary<TTypeId, Type> _idToTypes;
    private readonly Dictionary<Type,TTypeId> _typeToId;

    public StaticTypeResolver(IReadOnlyDictionary<TTypeId, Type> idToTypes)
    {
        _idToTypes = new Dictionary<TTypeId, Type>(idToTypes);
        _typeToId = idToTypes.ToDictionary(k => k.Value, v => v.Key);
    }
    
    public Optional<Type> Resolve(TTypeId id)
    {
        return _idToTypes.GetOptional(id);
    }

    public Optional<TTypeId> Resolve(Type type)
    {
        return _typeToId.GetOptional(type);
    }
}

public sealed class CombinedTypeResolver<TTypeId> : ITypeResolver<TTypeId>
{
    private readonly IReadOnlyList<ITypeResolver<TTypeId>> _resolvers;

    public CombinedTypeResolver(IEnumerable<ITypeResolver<TTypeId>> resolvers)
    {
        _resolvers = resolvers.ToList();
    }
    
    public Optional<Type> Resolve(TTypeId id)
    {
        return _resolvers.Select(r => r.Resolve(id)).FirstOrNone();
    }

    public Optional<TTypeId> Resolve(Type type)
    {
        return _resolvers.Select(r => r.Resolve(type)).FirstOrNone();
    }
}