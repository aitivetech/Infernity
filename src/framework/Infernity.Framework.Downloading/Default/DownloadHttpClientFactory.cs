namespace Infernity.Framework.Downloading.Default;

public sealed class DownloadHttpClientFactory : IDownloadHttpClientFactory
{
    public HttpClient Create()
    {
        return new HttpClient(new SocketsHttpHandler()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        });
    }
}
