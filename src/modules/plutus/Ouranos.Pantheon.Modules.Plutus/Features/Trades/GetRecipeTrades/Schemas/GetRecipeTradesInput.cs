using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;

public sealed record GetRecipeTradesInput(
    Id<Market> MarketId,
    double? Seconds = null
) : IQuery<WrapperResponse<IQueryable<GetRecipeTradesResponse>>>;
