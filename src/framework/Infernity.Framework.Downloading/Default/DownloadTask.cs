using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Framework.Core.Threading;
using Infernity.Framework.Security.Hashing;

namespace Infernity.Framework.Downloading.Default;

internal sealed class DownloadTask : Disposable, IDownloadTask
{
    private static readonly Dictionary<DownloadTaskState, HashSet<DownloadTaskState>> _allowedTransitions =
        new Dictionary<DownloadTaskState, HashSet<DownloadTaskState>>()
        {
            [DownloadTaskState.Active] = [DownloadTaskState.Queued],
            [DownloadTaskState.Succeeded] = [DownloadTaskState.Active],
            [DownloadTaskState.Failed] = [DownloadTaskState.Active],
            [DownloadTaskState.Cancelled] = [DownloadTaskState.Queued, DownloadTaskState.Active],
        };

    private static readonly HashSet<DownloadTaskState> _finalStates =
        [DownloadTaskState.Cancelled, DownloadTaskState.Succeeded, DownloadTaskState.Failed];

    private readonly AsyncLock _lock;
    private readonly DownloadManager _downloadManager;
    private readonly DownloadConfiguration _configuration;
    private DownloadTaskData _data;

    internal DownloadTask(
        DownloadManager downloadManager,
        DownloadConfiguration configuration,
        DownloadTaskData data,
        bool continueExisting)
    {
        _lock = new AsyncLock();
        _downloadManager = downloadManager;
        _configuration = configuration;
        ContinueExisting = continueExisting;
        _data = data;
    }

    public DownloadTaskId Id => _data.Id;
    public DownloadTaskState State =>  _data.State;
    public string Url => _data.Url;
    public string TargetPath => _data.TargetPath;
    public long Length => _data.Length;
    public long Position => _data.Position;
    public Optional<Sha256Value> Hash => _data.Hash;
    
    internal bool ContinueExisting { get; }

    public async Task Cancel()
    {
        async Task Notification()
        {
            await _configuration.Handler.OnCancelled(_downloadManager,
                this);
        }

        using var _ = await _lock.LockAsync();

        if (CanTransitionTo(DownloadTaskState.Cancelled))
        {
            await TransitionTo(DownloadTaskState.Cancelled,
                Notification);
        }
    }

    internal async Task Success()
    {
        async Task Notification()
        {
            await using var readStream = await _configuration.Storage.OpenRead(_data.TargetPath);

            await _configuration.Handler.OnSuccess(_downloadManager,
                this,
                readStream);
        }

        using var _ = await _lock.LockAsync();
        await TransitionTo(DownloadTaskState.Succeeded,
            Notification);
    }

    internal async Task Failed(Exception exception,
        int statusCode)
    {
        async Task Notification()
        {
            await _configuration.Handler.OnFailed(_downloadManager,
                this,
                exception,
                statusCode);
        }

        using var _ = await _lock.LockAsync();
        await TransitionTo(DownloadTaskState.Failed,
            Notification);
    }

    internal async Task Progress(long position)
    {
        using var _ = await _lock.LockAsync();

        if (_data.State == DownloadTaskState.Active)
        {
            _data = _data with { Position = position };
            await _configuration.Database.AddOrUpdate(_data);
            await _configuration.Handler.OnProgress(_downloadManager,
                this);
        }
    }

    protected override void OnDispose()
    {
        _lock.Dispose();
    }

    private bool CanTransitionTo(DownloadTaskState to)
    {
        return _allowedTransitions[to].Contains(_data.State);
    }

    private async Task TransitionTo(DownloadTaskState to,
        Func<Task>? preStateNotification = null,
        Func<Task>? postStateNotification = null)
    {
        if (!CanTransitionTo(to))
        {
            throw new InvalidOperationException($"Cannot transition from {_data.State} to {to}");
        }
        
        var willBeFinalState = _finalStates.Contains(to);

        _data = _data with { State = to };

        if (willBeFinalState)
        {
            _data = _data with { CompletedAt = DateTimeOffset.UtcNow };
        }

        await _configuration.Database.AddOrUpdate(this._data);

        if (preStateNotification != null)
        {
            await preStateNotification();
        }

        await _configuration.Handler.OnProgress(_downloadManager,
            this);

        if (postStateNotification != null)
        {
            await postStateNotification();
        }

        if (willBeFinalState)
        {
            await _configuration.Database.Remove(this.Id);

            if (to != DownloadTaskState.Succeeded)
            {
                await _configuration.Storage.Delete(TargetPath);
            }
            else
            {
                await _configuration.Storage.Publish(TargetPath);
            }
            
            _downloadManager.FinalizeTask(this);
        }
    }
}