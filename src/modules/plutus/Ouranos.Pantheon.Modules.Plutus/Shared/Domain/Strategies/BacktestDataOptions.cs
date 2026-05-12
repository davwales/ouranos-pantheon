namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record BacktestDataOptions(int LookbackDays)
{
    public const string SectionName = "BacktestData";

    public BacktestDataOptions()
        : this(LookbackDays: 30) { }
}
