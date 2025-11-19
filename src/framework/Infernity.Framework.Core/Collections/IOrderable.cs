namespace Infernity.Framework.Core.Collections;

public interface IOrderable
{
    int Order => int.MaxValue;
}

public static class OrderableExtensions
{
    public static IReadOnlyList<T> OrderOrderable<T>(this IEnumerable<T> items)
        where T : IOrderable => items.OrderBy(o => o.Order).ToList();
    
    public static IReadOnlyList<T> OrderOrderableOptionally<T>(this IEnumerable<T> items,int orderForUnordered = 0)
    {
        return items.Select(i =>
            {
                var order = orderForUnordered;

                if (i is IOrderable orderable)
                {
                    order = orderable.Order;
                }

                return (order, i);
            }).OrderBy(o => o.order)
            .Select(o => o.i).ToList();
    }
}
