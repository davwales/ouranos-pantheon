using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;

public sealed record GetRecipeTradesInput(
    Id<Market> MarketId,
    double? Seconds = null
) : IQuery<WrapperResponse<IQueryable<GetRecipeTradesResponse>>>;
