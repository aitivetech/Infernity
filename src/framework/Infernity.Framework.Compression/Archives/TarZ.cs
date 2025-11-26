using System.Formats.Tar;

using ZstdSharp;

namespace Infernity.Framework.Compression.Archives;

public static class TarZ
{
    public static async Task Create(
        string sourcePath,
        string targetPath,
        int level = 22,
        CancellationToken cancellationToken = default)
    {
        await using var targetFileStream = new FileStream(targetPath,
            FileMode.Create,FileAccess.Write);
        await using var compressionStream = new CompressionStream(targetFileStream,
            level,
            leaveOpen: true);

        await TarFile.CreateFromDirectoryAsync(sourcePath,
            compressionStream,
            false,
            cancellationToken);
    }

    public static async Task Extract(
        string sourcePath,
        string targetPath,
        CancellationToken cancellationToken = default
    )
    {
        await using var sourceFileStream = new FileStream(sourcePath,
            FileMode.Open,FileAccess.Read);

        await using var decompressionStream = new DecompressionStream(sourceFileStream,
            leaveOpen: true);

        await TarFile.ExtractToDirectoryAsync(decompressionStream,
            targetPath,true,
            cancellationToken);
    }
}