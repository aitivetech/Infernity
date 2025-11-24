using System.Threading.Channels;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns;
using Infernity.Framework.Core.Threading;
using Infernity.Framework.Security.Hashing;

namespace Infernity.Framework.Downloading.Default;

public sealed class DownloadManager : IDownloadManager
{
    private readonly AsyncLock _lock;
    private readonly DownloadConfiguration _configuration;
    private readonly IHashProvider<Sha256Value> _hashProvider;
    private readonly HttpClient _httpClient;
    private readonly Channel<DownloadTask> _taskChannel;
    private readonly Dictionary<string,DownloadTask>  _tasks;
    private readonly DownloadWorker[] _workers;
    private readonly Task[] _workerTasks;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public DownloadManager(DownloadConfiguration configuration,IHashProvider<Sha256Value>? hashProvider = null)
    {
        _lock = new AsyncLock();
        _configuration = configuration;
        _hashProvider = hashProvider ?? GlobalsRegistry.Resolve<IHashProvider<Sha256Value>>();
        _httpClient = configuration.ClientFactory.Create();
        _cancellationTokenSource = new CancellationTokenSource();
        _tasks = new Dictionary<string,DownloadTask>();
        _taskChannel = Channel.CreateUnbounded<DownloadTask>();
        _workers = Enumerable.Range(0,
            configuration.NumWorkers).Select(_ => new DownloadWorker(configuration,_hashProvider,
            _taskChannel.Reader)).ToArray();
        _workerTasks = _workers.Select(r => r.Run(_cancellationTokenSource.Token)).ToArray();
    }

    public async Task<IDownloadTask> AddTask(string url,
        string targetPath,
        IDownloadMetadataProvider metadataProvider,
        bool continueExisting)
    {
        using (var _ = await _lock.LockAsync())
        {
            if (_tasks.TryGetValue(url,
                    out var task1))
            {
                return task1;
            }
        }
        
        var metadata = await metadataProvider.GetMetadata(_httpClient,
            url);
        
        using var __ = await _lock.LockAsync();

        if (_tasks.TryGetValue(url,
                out var task2))
        {
            return task2;
        }

        var newId = DownloadTaskId.Compute(_hashProvider,
            url);

        var newData = new DownloadTaskData(newId,
            DownloadTaskState.Queued,
            DateTime.UtcNow,
            url,
            targetPath,
            metadata.Length,
            0L,
            metadata.Hash,
            Optional<DateTimeOffset>.None);
        
        return await AddTask(newData,continueExisting);
    }

    public async ValueTask DisposeAsync()
    {
        _taskChannel.Writer.Complete();
        await _cancellationTokenSource.CancelAsync();

        try
        {
            await Task.WhenAll(_workerTasks);
        }
        catch (AggregateException)
        {

        }
        catch (OperationCanceledException)
        {
            
        }

        _httpClient.Dispose();
        _cancellationTokenSource.Dispose();
        _lock.Dispose();
    }

    internal async Task<IDownloadTask> AddTask(DownloadTaskData data,bool continueExisting)
    {
        var newTask = new DownloadTask(this,_configuration,data,continueExisting);
        _tasks.Add(data.Url,newTask);
        await _taskChannel.Writer.WriteAsync(newTask);
        
        return newTask;
    }

    internal void FinalizeTask(DownloadTask task)
    {
        using var _ = _lock.Lock();
        
        if (_tasks.Remove(task.Url))
        {
            task.Dispose();
        }
    }
}