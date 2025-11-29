using Infernity.Framework.Security.Hashing;

namespace Infernity.Framework.Security.Io;

public static class FileHashingExtensions
{
    extension(FileInfo file)
    {
        public T CalculateHash<T>(IHashProvider<T> hashProvider)
            where T : struct,IHashValue<T>
        {
            using var stream = file.OpenRead();
            return hashProvider.Algorithm.Compute(stream);
        }
    }
    
}