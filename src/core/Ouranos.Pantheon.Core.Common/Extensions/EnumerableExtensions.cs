using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Core.Common.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(
        this IEnumerable<T> source,
        int batchSize
    )
    {
        Guard.Against.NegativeOrZero(batchSize);

        var batch = new List<T>(batchSize);
        foreach (var item in source)
        {
            batch.Add(item);

            if (batch.Count != batchSize)
            {
                continue;
            }

            yield return batch;
            batch = new List<T>(batchSize);
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}