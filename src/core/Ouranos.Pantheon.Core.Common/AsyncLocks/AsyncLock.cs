namespace Ouranos.Pantheon.Core.Common.AsyncLocks;

public sealed class AsyncLock(int initialCount, int maxCount) : IAsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(initialCount, maxCount);
    private bool _isDisposed;
    private int _refCount;

    public async Task<IDisposable> LockAsync(Action? onRelease = null)
    {
        Interlocked.Increment(ref _refCount);
        await _semaphore.WaitAsync();
        return new AsyncLockReleaser(this, onRelease);
    }

    internal void Release()
    {
        if (_isDisposed)
        {
            return;
        }

        _semaphore.Release();
        if (Interlocked.Decrement(ref _refCount) != 0)
        {
            return;
        }

        _semaphore.Dispose();
        _isDisposed = true;
    }
}