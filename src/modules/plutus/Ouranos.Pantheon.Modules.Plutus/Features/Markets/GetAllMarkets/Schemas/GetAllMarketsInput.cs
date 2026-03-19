using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;

public sealed record GetAllMarketsInput(
    string[]? Filter = null
) : IQuery<WrapperResponse<IQueryable<GetAllMarketsResponse>>>;
