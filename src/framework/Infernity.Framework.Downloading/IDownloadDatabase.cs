namespace Infernity.Framework.Downloading;

public interface IDownloadDatabase
{
    IAsyncEnumerable<DownloadTaskData> EnumerateTasks();
    
    Task<DownloadTaskData> GetTask(DownloadTaskId taskId);
    
    Task AddOrUpdate(DownloadTaskData data);
    
    Task Remove(DownloadTaskId downloadTaskId);
}