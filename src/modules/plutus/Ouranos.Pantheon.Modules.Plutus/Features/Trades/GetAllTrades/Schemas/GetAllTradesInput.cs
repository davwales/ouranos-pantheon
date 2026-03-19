namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;

public sealed record GetAllTradesInput(
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
);
