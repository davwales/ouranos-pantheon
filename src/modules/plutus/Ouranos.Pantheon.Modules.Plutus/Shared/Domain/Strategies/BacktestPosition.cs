namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record BacktestPosition(
    string SymbolId,
    string SymbolName,
    decimal EntryPrice,
    decimal ExitPrice,
    decimal Volume,
    decimal ProfitLoss,
    decimal ReturnPercent,
    DateTimeOffset EntryTime,
    DateTimeOffset ExitTime
)
{
    public BacktestPosition() : this(
        string.Empty,
        string.Empty,
        0,
        0,
        0,
        0,
        0,
        default,
        default
    )
    { }
}