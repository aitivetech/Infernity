using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Framework.Core.Threading;

using PathLib;

namespace Infernity.Inference.Abstractions.Models.Libraries;

internal sealed class LocalModelLibrary : IModelLibrary
{
    private readonly LocalModelLibraryConfiguration _configuration;
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly PosixPath _rootPath;
    
    public LocalModelLibrary(LocalModelLibraryConfiguration configuration)
    {
        _configuration = configuration;
        _rootPath = configuration.Path;
        _rootPath.EnsureDirectory();
    }

    public IReadOnlyDictionary<VersionedModelId, bool> GetAvailableModels()
    {
        using var _ = _lock.AcquireReadLock();
        
        var result = new Dictionary<VersionedModelId, bool>();
        var subDirectories = new DirectoryInfo(_rootPath.ToPosix()).EnumerateDirectories();

        foreach (var subDirectory in subDirectories)
        {
            if (VersionedModelId.TryParse(subDirectory.Name,null,
                    out var id))
            {
                var isComplete = !File.Exists(Path.Combine(subDirectory.FullName, ModelFileLayout.LockFileName));
                result.Add(id, isComplete);
            }
        }
        
        return result;
    }

    public Optional<IModelReader> Read(VersionedModelId id)
    {
        var readLock = _lock.AcquireReadLock();
        
        var path = GetModelPath(id);

        if (path.Exists())
        {
            return Optional.Some<IModelReader>(new LocalModelReader(id,
                path,
                readLock));
        }
        
        return Optional.None<IModelReader>();
    }

    public IModelWriter Write(VersionedModelId id)
    {
        using var writeLock = _lock.AcquireWriteLock();
        
        var modelPath = GetModelPath(id);
        modelPath.EnsureDirectory();

        return new LocalModelWriter(id,
            modelPath,
             new ActionDisposable(() => {}));
    }

    public void Delete(VersionedModelId id)
    {
        using var _ = _lock.AcquireWriteLock();
        
        var path = GetModelPath(id);

        if (path.Exists())
        {
            path.Delete();
        }
    }

    private PosixPath GetModelPath(VersionedModelId id)
    {
        return _rootPath / id.ToString();
    }
}