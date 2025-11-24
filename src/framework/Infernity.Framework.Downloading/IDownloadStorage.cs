namespace Infernity.Framework.Downloading;

public interface IDownloadStorage
{
    Task<Stream> OpenRead(string targetPath);
    Task<Stream> OpenWrite(string targetPath,bool overwrite);
    
    Task Publish(string targetPath);
    
    Task Delete(string targetPath);
}