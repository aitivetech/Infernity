using Infernity.Framework.Core.Io.Paths;

namespace Infernity.Inference.Abstractions.Models;

public interface IModelWriter : IModelReader
{
    PurePosixPath TempDirectoryPath { get; }
    
    PurePosixPath ManifestFilePath { get; }
    
    void Commit();
}