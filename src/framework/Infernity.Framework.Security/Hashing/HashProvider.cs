using System.Buffers;

using Microsoft.IO;

namespace Infernity.Framework.Security.Hashing;

public sealed class HashProvider<T>(
    IHashAlgorithm<T> algorithm,
    RecyclableMemoryStreamManager streamManager) : IHashProvider<T>
    where T : struct, IHashValue<T>
{
    public HashBuilder<T> CreateBuilder()
    {
        return new HashBuilder<T>(algorithm,
            streamManager.GetStream(),
            ArrayPool<byte>.Shared);
    }
}