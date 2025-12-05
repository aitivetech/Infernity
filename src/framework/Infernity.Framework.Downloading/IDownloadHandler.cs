namespace Infernity.Framework.Downloading;


public interface IDownloadHandler
{
    Task OnSuccess(
        IDownloadManager manager,
        IDownloadTask task,
        Stream readStream);

    Task OnFailed(
        IDownloadManager manager,
        IDownloadTask task,
        Exception ex,
        int statusCode);

    Task OnCancelled(
        IDownloadManager manager,
        IDownloadTask task);

    Task OnProgress(
        IDownloadManager manager,
        IDownloadTask task);
}