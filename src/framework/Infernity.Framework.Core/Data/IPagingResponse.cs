using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Data;

public static class PagingResponse
{
    extension(IOffsetPagingRequest request)
    {
        public IOffsetPagingResponse<T> Respond<T>(IReadOnlyList<T> items,long total)
        {
            var currentOffset = request.Start;

            var previousOffset = currentOffset >= 0L ? Math.Max(currentOffset - request.Limit,
                0L) : Optional.None<long>();
            
            var nextOffset = currentOffset + request.Limit + 1 < total ?  currentOffset + request.Limit : Optional.None<long>();
            
            if (items.Count < request.Limit)
            {
                // We have the last page
                nextOffset = Optional.None<long>();
                total = request.Start + items.Count;
            }
            
            return new OffsetPagingResponse<T>(items,
                total,
                previousOffset,
                currentOffset,
                nextOffset);
        }

        public IOffsetPagingResponse<T> RespondEmpty<T>()
        {
            return Respond<T>(request,[],
                0L);
        }
    }
}

public interface IPagingResponse
{
    long Total { get; }
}

public interface IPagingResponse<TCursor> : IPagingResponse
{
    Optional<TCursor> Next { get; }
    
    TCursor Current { get; }
    
    Optional<TCursor> Prev { get; }
}

public interface IPagingResponse<TCursor,out T> : IPagingResponse<TCursor>
{
    IReadOnlyList<T> Items { get; }
}

public interface IOffsetPagingResponse<out T> : IPagingResponse<long, T>;

internal sealed record OffsetPagingResponse<T>(
    IReadOnlyList<T> Items,
    long Total,
    Optional<long> Prev,
    long Current,
    Optional<long> Next) : IOffsetPagingResponse<T>
{
    
}