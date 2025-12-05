namespace Infernity.Framework.Core.Data;

public interface IQueryHandler<T, TCursor,TPagingRequest, in TQuery>
    where TPagingRequest : IPagingRequest<TCursor>
{
    
    Task<IPagingResponse<TCursor,T>> Query(
        TQuery query,
        TPagingRequest pagingRequest,
        CancellationToken cancellationToken = default);
}

public interface IOffsetQueryHandler<T, in TQuery> : IQueryHandler<T, long,IOffsetPagingRequest, TQuery>
{
}