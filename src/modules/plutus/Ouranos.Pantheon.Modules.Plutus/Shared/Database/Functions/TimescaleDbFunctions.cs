using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.Functions;

public static class TimescaleDbFunctions
{
    public static DateTime TimeBucket(TimeSpan interval, DateTime timestamp)
    {
        var ticks = timestamp.Ticks / interval.Ticks * interval.Ticks;
        return new DateTime(ticks, timestamp.Kind);
    }

    public static DateTimeOffset TimeBucket(TimeSpan interval, DateTimeOffset timestamp)
    {
        var ticks = timestamp.UtcTicks / interval.Ticks * interval.Ticks;
        return new DateTimeOffset(ticks, TimeSpan.Zero);
    }

    public static string ToTimescaleInterval(this TimeSpan timeSpan)
    {
        // Use the largest unit that represents the span exactly: truncating (e.g. 43 hours to
        // "1 days") would bucket differently from the TimeBucket translation of the same span.
        if (timeSpan.Ticks % TimeSpan.TicksPerDay == 0)
        {
            return $"{(long)timeSpan.TotalDays} days";
        }

        if (timeSpan.Ticks % TimeSpan.TicksPerHour == 0)
        {
            return $"{(long)timeSpan.TotalHours} hours";
        }

        if (timeSpan.Ticks % TimeSpan.TicksPerMinute == 0)
        {
            return $"{(long)timeSpan.TotalMinutes} minutes";
        }

        return $"{(long)timeSpan.TotalSeconds} seconds";
    }

    public static void HasTimescaleDbFunctions(this ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDbFunction(() => TimeBucket(TimeSpan.Zero, DateTime.UtcNow))
            .HasName("time_bucket")
            .HasTranslation(args => new SqlFunctionExpression(
                "time_bucket",
                args,
                true,
                [false, true],
                typeof(DateTime),
                null
            ));

        modelBuilder
            .HasDbFunction(() => TimeBucket(TimeSpan.Zero, DateTimeOffset.UtcNow))
            .HasName("time_bucket")
            .HasTranslation(args => new SqlFunctionExpression(
                "time_bucket",
                args,
                true,
                [false, true],
                typeof(DateTimeOffset),
                null
            ));
    }
}
