namespace Ouranos.Pantheon.Core.Common.AsyncLocks;

public interface IKeyedAsyncLock<TKey> where TKey : notnull
{
    Task<IDisposable> LockAsync(TKey key);
}