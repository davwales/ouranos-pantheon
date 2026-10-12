namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

public static class LiquidityMath
{
    public const int TypicalVolumeDays = 14;

    /// <summary>
    ///     Median daily volume over <paramref name="days" /> days, given the volumes of the days
    ///     that had trades; days without trades count as zero.
    /// </summary>
    public static decimal MedianDailyVolume(IEnumerable<decimal> tradedDayVolumes, int days)
    {
        var volumes = new decimal[days];
        var i = 0;

        foreach (var volume in tradedDayVolumes)
        {
            if (i == days)
            {
                break;
            }

            volumes[i++] = volume;
        }

        Array.Sort(volumes);
        var mid = days / 2;
        return days % 2 == 0 ? (volumes[mid - 1] + volumes[mid]) / 2m : volumes[mid];
    }

    /// <summary>Units that can realistically be traded given today's and the typical volume.</summary>
    public static decimal FillCap(
        decimal dayVolume,
        decimal typicalVolume,
        decimal participationRate
    )
    {
        if (dayVolume <= 0)
        {
            return 0m;
        }

        return Math.Floor(Math.Min(dayVolume, typicalVolume) * participationRate);
    }
}
