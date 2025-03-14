using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Recipes.GetRecipeTrades;

public sealed record GetRecipeTradesInput(
    Id<Market> MarketId,
    double? Seconds = null
) : IQuery<WrapperResponse<IQueryable<GetRecipeTradesResponse>>>;