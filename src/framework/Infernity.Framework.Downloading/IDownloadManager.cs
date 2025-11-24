using System.Formats.Tar;

using Infernity.Framework.Downloading.Default;
using Infernity.Framework.Security.Hashing;

namespace Infernity.Framework.Downloading;

public interface IDownloadManager : IAsyncDisposable
{
    Task<IDownloadTask> AddTask(
        string url,
        string targetPath,
        IDownloadMetadataProvider metadataProvider,
        bool continueExisting);
    
    static async Task<IDownloadManager> Create(
        DownloadConfiguration configuration,
        bool restore,
        IHashProvider<Sha256Value> ? hashProvider = null)
    {
        var result = new DownloadManager(configuration,
            hashProvider);

        await foreach (var taskData in configuration.Database.EnumerateTasks().OrderBy(t => t.CreatedAt))
        {
            if (taskData.State == DownloadTaskState.Active || taskData.State == DownloadTaskState.Queued)
            {
                await result.AddTask(taskData,
                    true);
            }
        }
        
        return result;
    }
}