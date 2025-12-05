using System.Globalization;

using Infernity.Framework.Core.Functional;

namespace Infernity.Inference.Abstractions.Models;

public interface IModelLibrary
{
    IReadOnlyDictionary<VersionedModelId,bool> GetAvailableModels();
    
    Optional<IModelReader> Read(VersionedModelId id);

    IModelWriter Write(VersionedModelId id);
    
    void Delete(VersionedModelId id);
}