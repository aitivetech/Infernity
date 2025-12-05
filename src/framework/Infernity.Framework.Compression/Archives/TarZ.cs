using System.Formats.Tar;
using System.IO.Enumeration;
using System.Numerics;

using Infernity.Framework.Core.Io.Streams;

using ZstdSharp;

namespace Infernity.Framework.Compression.Archives;

public static class TarZ
{
    private static readonly StreamProgressHandler _defaultProgressHandler = (stream,total,current) =>
    {
    };
    
    public static async Task Create(
        string sourcePath,
        string targetPath,
        StreamProgressHandler? progressHandler = null,
        int compressionLevel = 22,
        CancellationToken cancellationToken = default)
    {
        // Fixed timestamp for reproducibility
        var fixedTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    
        // Get all files sorted consistently
        var files = Directory.GetFiles(sourcePath,
                "*",
                SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(sourcePath,
                f))
            .OrderBy(f => f,
                StringComparer.Ordinal);

        await using var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
        await using var compressionStream = new CompressionStream(targetStream, compressionLevel,leaveOpen:true);
        await using var progressStream = new ProgressStream(compressionStream,
            progressHandler ?? _defaultProgressHandler,
            true);
        await using var tarWriter = new TarWriter(progressStream, TarEntryFormat.Pax, leaveOpen: true);

        foreach (var relativePath in files)
        {
            var fullPath = Path.Combine(sourcePath, relativePath);
        
            // Create entry manually to control all metadata
            var entry = new PaxTarEntry(TarEntryType.RegularFile, relativePath)
            {
                ModificationTime = fixedTime,
                Uid = 0,
                Gid = 0,
                UserName = "",
                GroupName = "",
                Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite | 
                       UnixFileMode.GroupRead | UnixFileMode.OtherRead,
                // Set the data source
                DataStream = File.OpenRead(fullPath) // 0644
            };

            await tarWriter.WriteEntryAsync(entry,cancellationToken);
        
            // Don't forget to dispose the stream
            await entry.DataStream.DisposeAsync();
        }
    }

    public static async Task Extract(string sourcePath,
        string targetPath,
        StreamProgressHandler? progressHandler = null,
        CancellationToken cancellationToken = default)
    {
        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        await using var progressStream = new ProgressStream(sourceStream,_defaultProgressHandler,true);
        await using var decompressionStream = new DecompressionStream(progressStream,leaveOpen:true);
       
        await TarFile.ExtractToDirectoryAsync(decompressionStream,targetPath,true,cancellationToken);
    }
}