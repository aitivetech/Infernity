using Infernity.Framework.Downloading.Databases;
using Infernity.Framework.Downloading.Default;
using Infernity.Framework.Downloading.Storage;

namespace Infernity.Framework.Downloading;

public sealed class DownloadConfiguration
{
    public DownloadConfiguration()
    {
        ChunkSize = 1024 * 1024;
        NumWorkers = 4;
        ClientFactory = new DownloadHttpClientFactory();
    }

    public DownloadConfiguration(
        string targetPath,
        string databasePath,
        IDownloadHandler handler,
        int chunkSize = 1024 * 1024,
        int numWorkers = 4) : this()
    {
        Handler = handler;
        Storage = new DirectoryDownloadStorage(targetPath);
        Database = new FileDownloadDatabase(databasePath);
        
        ChunkSize = chunkSize;
        NumWorkers = numWorkers;
    }
    
    public int ChunkSize { get; init; }
    public int NumWorkers { get; init; }

    public required IDownloadHandler Handler { get; init; }
    
    public required IDownloadStorage Storage { get; init; }
    
    public required IDownloadDatabase Database { get; init; }

    public IDownloadHttpClientFactory ClientFactory { get; init; }
}