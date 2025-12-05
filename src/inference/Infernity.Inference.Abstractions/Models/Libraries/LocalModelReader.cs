using Infernity.Framework.Core.Io.Paths;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Inference.Abstractions.Models.Manifest;

using PathLib;

namespace Infernity.Inference.Abstractions.Models.Libraries;

internal class LocalModelReader : Disposable,IModelReader
{
    private readonly PurePosixPath _rootPath;
    private readonly IDisposable _lock;
    
    internal LocalModelReader(
        VersionedModelId id,
        PurePosixPath rootPath,
        IDisposable lockObject)
    {
        Id = id;
        _rootPath = rootPath;
        _lock = lockObject;
    }

    public VersionedModelId Id { get; }

    public bool IsComplete => !LockFilePath.Exists();
    
    public ModelManifest Manifest => ModelManifest.Read(ResolvePath(ModelFileLayout.ManifestFileName).ToPosix());
    
    public PurePosixPath DataDirectoryPath => _rootPath / ModelFileLayout.DataDirectoryPath;

    protected PosixPath  LockFilePath => _rootPath / ModelFileLayout.LockFileName;
    
    protected override void OnDispose()
    {
        _lock.Dispose();
    }

    protected PosixPath ResolvePath(string relativePath)
    {
        return(_rootPath / relativePath);
    }
}