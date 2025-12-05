using Infernity.Framework.Core.Patterns.Disposal;

namespace Infernity.Framework.Core.Threading;

public static class LockExtensions
{
    extension(ReaderWriterLockSlim readerWriterLockSlim)
    {
        public IDisposable AcquireReadLock()
        {
            readerWriterLockSlim.EnterReadLock();
            return new ActionDisposable(readerWriterLockSlim.ExitReadLock);
        }

        public IDisposable AcquireWriteLock()
        {
            readerWriterLockSlim.EnterWriteLock();
            return new ActionDisposable(readerWriterLockSlim.ExitWriteLock);
        }
    }
}