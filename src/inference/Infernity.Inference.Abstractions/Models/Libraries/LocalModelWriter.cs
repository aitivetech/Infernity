using Infernity.Framework.Core.Io.Paths;

using PathLib;

namespace Infernity.Inference.Abstractions.Models.Libraries;

internal sealed class LocalModelWriter : LocalModelReader,IModelWriter
{
    internal LocalModelWriter(
        VersionedModelId id, 
        PurePosixPath rootPath, 
        IDisposable lockObject) 
        : base(id, rootPath, lockObject)
    {
        PosixPath tempPath = TempDirectoryPath;
        tempPath.EnsureDirectory();
        
        PosixPath dataPath = DataDirectoryPath;
        dataPath.EnsureDirectory();

        using var lockFileStream = new FileStream(LockFilePath.ToPosix(),FileMode.Create,FileAccess.Write,FileShare.None);
        lockFileStream.Write(BitConverter.GetBytes(DateTimeOffset.UnixEpoch.ToUnixTimeMilliseconds()));
    }

    public PurePosixPath TempDirectoryPath => ResolvePath("temp");
    public PurePosixPath ManifestFilePath => ResolvePath(ModelFileLayout.ManifestFileName);
    
    public void Commit()
    {
        PosixPath tempPath = TempDirectoryPath;
        tempPath.Delete();
        
        LockFilePath.Delete();
    }
}