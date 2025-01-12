using System.Collections.Concurrent;

namespace Ouranos.Pantheon.Core.Common.AsyncLocks;

public sealed class KeyedAsyncLock<TKey>(int initialCount = 1, int maxCount = 1)
    : IKeyedAsyncLock<TKey> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, AsyncLock> _locks = [];

    public async Task<IDisposable> LockAsync(TKey key)
    {
        var asyncLock = _locks.GetOrAdd(key, _ => new AsyncLock(initialCount, maxCount));
        return await asyncLock.LockAsync(() => _locks.TryRemove(key, out _));
    }
}