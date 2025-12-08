using Infernity.Framework.Core.Data;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions.Models;

public interface IModelFeed
{
    Task<IReadOnlyDictionary<ModelId, IReadOnlyList<ModelManifest>>> GetAvailableModels(
        bool includeOnlyLatest,
        CancellationToken token = default);

    Task Install(IModelLibrary target,
        VersionedModelId modelId,
        CancellationToken cancellationToken = default);

    Task Uninstall(IModelLibrary target,
        VersionedModelId modelId,
        CancellationToken token = default);
}