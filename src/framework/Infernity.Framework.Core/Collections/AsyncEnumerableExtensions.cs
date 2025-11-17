using System.Runtime.CompilerServices;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Collections;

public static class AsyncEnumerableExtensions
{
    extension<T>(IAsyncEnumerable<T> a)
    {
        public async Task<Optional<T>> FindFirstCommonElement(IAsyncEnumerable<T> b,
            IEqualityComparer<T>? equalityComparer = null, 
            CancellationToken cancellationToken = default)
        {
            var actualEqualityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        
            var bufferA = new HashSet<T>(actualEqualityComparer);
            var bufferB = new HashSet<T>(actualEqualityComparer);

            await using var enumeratorA = a.GetAsyncEnumerator(cancellationToken);
            await using var enumeratorB = b.GetAsyncEnumerator(cancellationToken);
        
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!await enumeratorA.MoveNextAsync() || !await enumeratorB.MoveNextAsync())
                {
                    break;
                }

                // We have two new elements.
                var elementA = enumeratorA.Current;
                var elementB = enumeratorB.Current;

                bufferA.Add(elementA);
                bufferB.Add(elementB);
            
                if (bufferA.Contains(elementB))
                {
                    return elementB;
                }

                if (bufferB.Contains(elementA))
                {
                    return elementA;
                }
            }
        
            return Optional<T>.None;
        }

        public async IAsyncEnumerable<List<T>> Chunk(int chunkSize,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var chunk = new List<T>(chunkSize);
        
            await foreach (var item in a.WithCancellation(cancellationToken))
            {
                chunk.Add(item);

                if (chunk.Count == chunkSize)
                {
                    yield return chunk;
                    chunk = new List<T>(chunkSize);
                }
            }

            if (chunk.Any())
            {
                yield return chunk;
            }
        }
    }
}