using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;

public sealed record GetRecipeTradesInput(
    Id<Market> MarketId,
    TimeFrame TimeFrame = TimeFrame.OneHour,
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
);
