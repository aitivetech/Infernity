using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Data;

public static class PagingRequest
{
    public const int DefaultLimit = 32;
    
    public static IOffsetPagingRequest FromOffset(long offset,int limit = DefaultLimit)
        => new OffsetPagingRequest(offset, limit);
}

public interface IPagingRequest<TCursor>
{
    TCursor Start { get; }
    
    int Limit { get; }
}

public interface IOffsetPagingRequest : IPagingRequest<long>
{
    
}

internal sealed record OffsetPagingRequest(
    long Start,
    int Limit) : IOffsetPagingRequest
{
   
}