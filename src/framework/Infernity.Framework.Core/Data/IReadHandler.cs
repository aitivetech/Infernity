using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Data;

public interface IReadHandler<TId,T> where TId : notnull
{
    Task<Optional<T>> ReadById(TId id,CancellationToken cancellationToken = default);
    
    async Task<IReadOnlyDictionary<TId,T>> ReadById(IReadOnlySet<TId> ids,CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<TId, T>();
        
        // Emulation implementation
        foreach (var id in ids)
        {
            var item = await ReadById(id,cancellationToken);

            if (item)
            {
                result.Add(id,item.Value);
            }
        }
        
        return result;
    }
}