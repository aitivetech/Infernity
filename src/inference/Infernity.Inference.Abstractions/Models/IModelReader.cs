using Infernity.Framework.Core.Io.Paths;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions.Models;

public interface IModelReader : IDisposable
{
    VersionedModelId Id { get; }
    
    bool IsComplete { get; }
    
    ModelManifest Manifest { get; }
    
    PurePosixPath DataDirectoryPath { get; }
}