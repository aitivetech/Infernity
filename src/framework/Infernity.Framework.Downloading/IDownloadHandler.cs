namespace Infernity.Framework.Downloading;

public enum DownloadCompletionBehavior
{
    None,
    Remove,
    Retry
}

public interface IDownloadHandler
{
    Task<DownloadCompletionBehavior> OnSuccess(
        IDownloadManager manager,
        IDownloadTask task,
        Stream readStream);

    Task<DownloadCompletionBehavior> OnFailed(
        IDownloadManager manager,
        IDownloadTask task,
        Exception ex,
        int statusCode);

    Task<DownloadCompletionBehavior> OnCancelled(
        IDownloadManager manager,
        IDownloadTask task);

    Task OnProgress(
        IDownloadManager manager,
        IDownloadTask task);
}