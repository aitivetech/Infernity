using Infernity.Framework.Core.Data;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Versioning;

namespace Infernity.Inference.Abstractions.Models;

public sealed record ModelFeedQuery();

public interface IModelFeed : IOffsetQueryHandler<ModelDescription, ModelFeedQuery>,IReadHandler<VersionedModelId,ModelDescription>
{
    Task Install(IModelLibrary target,
        VersionedModelId modelId,
        CancellationToken cancellationToken = default);

    Task Uninstall(IModelLibrary target,
        VersionedModelId modelId,
        CancellationToken token = default);
}