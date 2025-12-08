using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Threading;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions.Models.Feeds;

public abstract class ModelFeed : IModelFeed
{
    protected sealed record ModelFeedResponse(
        IReadOnlyDictionary<ModelId, IReadOnlyList<ModelManifest>> Models,
        bool OnlyLatest,
        Optional<ConcurrencyToken> ConcurrencyToken);

    private readonly AsyncLock _lock;
    private Optional<ModelFeedResponse> _cachedResponse;

    protected ModelFeed()
    {
        _lock = new AsyncLock();
        _cachedResponse = Optional<ModelFeedResponse>.None;
    }

    public async Task<IReadOnlyDictionary<ModelId, IReadOnlyList<ModelManifest>>> GetAvailableModels(
        bool includeOnlyLatest,
        CancellationToken cancellationToken = default)
    {
        using var _ = await _lock.LockAsync(cancellationToken);

        if (_cachedResponse)
        {
            
        }
        else
        {
            _cachedResponse = await OnLoadManifests(Optional<ModelFeedResponse>.None,
                includeOnlyLatest,
                cancellationToken);
        }
    }

    public Task Install(IModelLibrary target,
        VersionedModelId modelId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Uninstall(IModelLibrary target,
        VersionedModelId modelId,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    protected abstract Task<ModelFeedResponse> OnLoadManifests(
        Optional<ModelFeedResponse> lastResponse,
        bool includeOnlyLatest,
        CancellationToken cancellationToken);
}