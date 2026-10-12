namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <param name="MinDailyValue">
///     Median value traded per day a symbol needs, over the <paramref name="UniverseDays" /> before the
///     data starts, to be considered for flipping.
/// </param>
/// <param name="UniverseDays">Days of history the flip universe is chosen from.</param>
/// <param name="QueryTimeoutMinutes">Command timeout for loading 5-minute side data.</param>
public sealed record FlipDataOptions(
    decimal MinDailyValue,
    int UniverseDays,
    int QueryTimeoutMinutes
)
{
    public const string SectionName = "FlipData";

    public FlipDataOptions()
        : this(MinDailyValue: 50_000_000m, UniverseDays: 30, QueryTimeoutMinutes: 15) { }
}
