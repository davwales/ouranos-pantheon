namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.Shared;

internal static class SmartIntervalCalculator
{
    /// <summary>
    /// Calculates a bucket interval that produces approximately <paramref name="numBuckets"/>
    /// buckets for the given <paramref name="duration"/>, rounded to human-friendly increments.
    /// </summary>
    public static TimeSpan Calculate(TimeSpan duration, int numBuckets)
    {
        if (duration.TotalSeconds <= 0)
        {
            return TimeSpan.FromMinutes(5);
        }

        var targetBucketSize = duration.TotalSeconds / numBuckets;

        return targetBucketSize switch
        {
            <= 60 => TimeSpan.FromSeconds(Math.Max(10, Math.Round(targetBucketSize))),
            <= 3600 => TimeSpan.FromMinutes(Math.Max(1, Math.Floor(targetBucketSize / 60))),
            <= 86400 => TimeSpan.FromMinutes(Math.Max(5, Math.Floor(targetBucketSize / 3600) * 60)),
            <= 604800 => TimeSpan.FromHours(Math.Max(1, Math.Floor(targetBucketSize / 3600))),
            <= 2592000 => TimeSpan.FromHours(
                Math.Max(6, Math.Floor(targetBucketSize / 86400) * 24)
            ),
            <= 31536000 => TimeSpan.FromDays(Math.Max(1, Math.Floor(targetBucketSize / 86400))),
            _ => TimeSpan.FromDays(Math.Max(7, Math.Floor(targetBucketSize / 604800) * 7)),
        };
    }
}
