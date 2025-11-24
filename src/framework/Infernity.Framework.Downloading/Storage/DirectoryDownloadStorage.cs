using System.Formats.Tar;

namespace Infernity.Framework.Downloading.Storage;

public class DirectoryDownloadStorage : IDownloadStorage
{
    private readonly string _path;

    public DirectoryDownloadStorage(string path)
    {
        Directory.CreateDirectory(path);
        _path = path;
    }

    public async Task<Stream> OpenRead(string targetPath)
    {
        return File.OpenRead(GetTempPath(targetPath));
    }

    public async Task<Stream> OpenWrite(string targetPath,bool overwrite)
    {
        if (overwrite)
        {
            return new FileStream(GetTempPath(targetPath), FileMode.Create, FileAccess.Write, FileShare.Read);
        }
        
        return new FileStream(GetTempPath(targetPath), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
    }

    public async Task Publish(string targetPath)
    {
        File.Move(GetTempPath(targetPath), GetFullPath(targetPath), true);
    }

    public async Task Delete(string targetPath)
    {
        File.Delete(GetTempPath(targetPath));
    }

    private string GetFullPath(string targetPath)
    {
        return Path.Combine(_path, targetPath);
    }

    private string GetTempPath(string targetPath)
    {
        return Path.Combine(_path, targetPath + ".download");
    }
}