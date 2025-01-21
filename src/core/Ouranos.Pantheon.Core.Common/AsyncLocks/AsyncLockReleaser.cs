using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Core.Common.AsyncLocks;

internal sealed class AsyncLockReleaser : IDisposable
{
    private readonly AsyncLock _asyncLock;
    private readonly Action? _onRelease;
    private bool _disposed;

    public AsyncLockReleaser(AsyncLock asyncLock, Action? onRelease = null)
    {
        Guard.Against.Null(asyncLock);

        _asyncLock = asyncLock;
        _onRelease = onRelease;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _asyncLock.Release();
        _disposed = true;
        _onRelease?.Invoke();
    }
}