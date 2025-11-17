using System.Collections;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Collections;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Of<T>(T value)
    {
        yield return value;
    }

    extension(IEnumerable enumerable)
    {
        public bool Any()
        {
            var enumerator = enumerable.GetEnumerator();

            try
            {
                return enumerator.MoveNext();
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public bool None()
        {
            return !enumerable.Any();
        }
    }

    extension<T>(IEnumerable<T> source)
    {
        public Optional<T> FirstOrNone()
        {
            using var enumerator = source.GetEnumerator();

            return enumerator.MoveNext() ? enumerator.Current : Optional<T>.None;
        }

        public bool None()
        {
            return !source.Any();
        }
    }

    extension<T>(IEnumerable<T?> enumerable) where T : class
    {
        public IEnumerable<T> NotNull()
        {
            return enumerable.Where(e => e != null).Select(e => e!);
        }
    }

    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<IReadOnlyList<T>> SlidingWindow(int windowSize,
            int stride)
        {
            var buffer = new LinkedList<T>();

            foreach (var item in source)
            {
                buffer.AddLast(item);

                if (buffer.Count == windowSize)
                {
                    yield return buffer.ToList();

                    // Remove stride items
                    for (var i = 0; i < stride; ++i)
                    {
                        buffer.RemoveFirst();
                    }
                }
            }
        }

        public async Task ForEach(Func<T, Task> body,
            CancellationToken cancellationToken = default)
        {
            foreach (var item in source)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await body.Invoke(item);
            }
        }
    }
}