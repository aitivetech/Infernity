using Infernity.Framework.Core.Functional;
using Infernity.Framework.Security.Hashing;

namespace Infernity.Framework.Downloading;

public sealed record DownloadMetadata(
    long Length,
    Optional<Sha256Value> Hash);

public interface IDownloadMetadataProvider
{
   Task<DownloadMetadata> GetMetadata(HttpClient httpClient,string url);
}