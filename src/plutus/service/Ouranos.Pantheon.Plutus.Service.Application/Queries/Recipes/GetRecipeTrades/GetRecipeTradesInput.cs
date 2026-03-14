using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Recipes.GetRecipeTrades;

public sealed record GetRecipeTradesInput(
    Id<Market> MarketId,
    double? Seconds = null
) : IQuery<WrapperResponse<IQueryable<GetRecipeTradesResponse>>>;