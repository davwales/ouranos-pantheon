namespace Ouranos.Pantheon.Core.Common.AsyncLocks;

public interface IAsyncLock
{
    Task<IDisposable> LockAsync(Action? onRelease = null);
}