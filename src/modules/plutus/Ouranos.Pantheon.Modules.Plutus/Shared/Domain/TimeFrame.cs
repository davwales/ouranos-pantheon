namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain;

public enum TimeFrame
{
    FifteenMinutes,
    OneHour,
    FourHours,
    OneDay,
    OneWeek,
    OneMonth,
    SixMonths,
    OneYear,
    AllTime,
}

public static class TimeFrameExtensions
{
    public static TimeSpan? ToTimeSpan(this TimeFrame frame)
    {
        return frame switch
        {
            TimeFrame.FifteenMinutes => TimeSpan.FromMinutes(15),
            TimeFrame.OneHour => TimeSpan.FromHours(1),
            TimeFrame.FourHours => TimeSpan.FromHours(4),
            TimeFrame.OneDay => TimeSpan.FromHours(24),
            TimeFrame.OneWeek => TimeSpan.FromDays(7),
            TimeFrame.OneMonth => TimeSpan.FromDays(30),
            TimeFrame.SixMonths => TimeSpan.FromDays(182),
            TimeFrame.OneYear => TimeSpan.FromDays(365),
            TimeFrame.AllTime => null,
            _ => throw new ArgumentOutOfRangeException(nameof(frame)),
        };
    }
}
