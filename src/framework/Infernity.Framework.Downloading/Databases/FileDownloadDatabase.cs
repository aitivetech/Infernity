using System.Text.Json;

using Infernity.Framework.Json;

namespace Infernity.Framework.Downloading.Databases;

public sealed class FileDownloadDatabase : IDownloadDatabase
{
    private const string _extension = ".download";
    
    private readonly string _path;
    
    public FileDownloadDatabase(string path)
    {
        Directory.CreateDirectory(path);
        _path = path;
    }
    
    public async IAsyncEnumerable<DownloadTaskData> EnumerateTasks()
    {
        foreach (var entry in Directory.EnumerateFiles(_path,
                     $"*.{_extension}",
                     SearchOption.TopDirectoryOnly))
        {
            yield return Read(entry);
        }
    }

    public async Task<DownloadTaskData> GetTask(DownloadTaskId taskId)
    {
        return Read(GetPath(taskId));
    }

    public async Task AddOrUpdate(DownloadTaskData data)
    {
        var path = GetPath(data.Id);
        
        JsonSerializer.SerializeToFile<DownloadTaskData>(path, data);
    }

    public async Task Remove(DownloadTaskId downloadTaskId)
    {
        File.Delete(GetPath(downloadTaskId));
    }

    private DownloadTaskData Read(string path)
    {
        return JsonSerializer.DeserializeFromFile<DownloadTaskData>(GetPath(path))!;
    }
    
    private string GetPath(DownloadTaskId downloadTaskId)
    {
        return Path.Combine(_path, downloadTaskId.ToString() + _extension);
    }
}