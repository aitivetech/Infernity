using Infernity.Framework.Security.Hashing;

namespace Infernity.Framework.Security.Io;

public sealed record DirectoryMetadata<T>(
    T Hash,
    long FileCount,
    long Size)
    where T : struct, IHashValue<T>;

public static class DirectoryHashingExtensions
{
    extension(DirectoryInfo directoryInfo)
    {
        public DirectoryMetadata<T> CalculateHashAndMetadata<T>(
            IHashProvider<T> hashProvider,
            bool recursive = true) where T : struct, IHashValue<T>
        {
            using var hashBuilder = hashProvider.CreateBuilder();
            var fileCount = 0L;
            var size = 0L;
            
            foreach (var entry in directoryInfo.GetFileSystemInfos().OrderBy(o => o.Name,
                         StringComparer.InvariantCultureIgnoreCase))
            {
                if (entry is FileInfo fileEntry)
                {
                    hashBuilder.Write(fileEntry.Name);
                    hashBuilder.Write(new FileInfo(fileEntry.FullName).CalculateHash(hashProvider));
                    fileCount++;
                    size += fileEntry.Length;
                }
                else if (entry is DirectoryInfo directoryEntry && recursive)
                {
                    hashBuilder.Write(entry.Name);
                    
                    var metadata = directoryEntry.CalculateHashAndMetadata<T>(hashProvider);
                    
                    hashBuilder.Write(metadata.Hash);
                    fileCount += metadata.FileCount;
                    size += metadata.Size;
                }
            }
            
            return new(hashBuilder.Build(),fileCount, size);
        }
    }
}