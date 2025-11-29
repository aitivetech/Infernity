using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Reflection;

public class DictionaryTypeResolver<TType,T> : ITypeResolver<TType> 
    where TType : notnull
{
    private readonly Dictionary<TType, Type> _idToTypes;
    private readonly Dictionary<Type,TType> _typeToIds;
    
    public DictionaryTypeResolver(IEnumerable<T> services,
        Func<T, TType> typeIdResolver,
        Func<T, Type> typeResolver)
    {
        _idToTypes = services.ToDictionary(typeIdResolver, typeResolver);
        _typeToIds = _idToTypes.ToDictionary(kv => kv.Value, kv => kv.Key);
    }
    
    public Optional<Type> Resolve(TType id)
    {
        return _idToTypes.GetOptional(id);
    }

    public Optional<TType> Resolve(Type type)
    {
        return _typeToIds.GetOptional(type);
    }
}