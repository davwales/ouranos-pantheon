using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Ouranos.Pantheon.Core.Infra.Postgres.Functions;

public static class TimescaleDbFunctions
{
    public static DateTime TimeBucket(TimeSpan interval, DateTime timestamp)
    {
        throw new InvalidOperationException("This method is for use in EF Core queries only");
    }

    public static DateTimeOffset TimeBucket(TimeSpan interval, DateTimeOffset timestamp)
    {
        throw new InvalidOperationException("This method is for use in EF Core queries only");
    }

    public static string ToTimescaleInterval(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
        {
            return $"{(int)timeSpan.TotalDays} days";
        }

        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours} hours";
        }

        if (timeSpan.TotalMinutes >= 1)
        {
            return $"{(int)timeSpan.TotalMinutes} minutes";
        }

        return $"{(int)timeSpan.TotalSeconds} seconds";
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
                )
            );

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
                )
            );
    }
}