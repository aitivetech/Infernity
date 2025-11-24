using Infernity.Framework.Security.Hashing;
using Infernity.GeneratedCode;

namespace Infernity.Framework.Downloading;

[TypedId]
public readonly partial record struct DownloadTaskId(Sha256Value Value)
{
    public static DownloadTaskId Compute(IHashProvider<Sha256Value> hashProvider,string url)
    {
        using var builder = hashProvider.CreateBuilder();
        builder.Write(url);
        return builder.Build();
    }
}