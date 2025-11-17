namespace Infernity.Framework.Core.Collections;

public interface IOrderable
{
    int Order => int.MaxValue;
}

public static class OrderableExtensions
{
    public static IReadOnlyList<T> OrderOrderable<T>(this IEnumerable<T> items)
        where T : IOrderable => items.OrderBy(o => o.Order).ToList();
    
    public static IReadOnlyList<T> OrderOrderableOptionally<T>(this IEnumerable<T> items,bool placeUnorderedFirst = true)
    
    {
        var realizedItems = items.ToList();
        var unordered = realizedItems.Where(i =>  i is IOrderable);
        var ordered = realizedItems.OfType<IOrderable>().OrderBy(o => o.Order).Cast<T>();
        
        return placeUnorderedFirst 
            ? unordered.Concat(ordered).ToList() : ordered.Concat(unordered).ToList();
    }
}
