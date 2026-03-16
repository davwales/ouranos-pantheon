using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;

public sealed record UpdateMarketInput(
    Id<Market> MarketId,
    string Name,
    Taxes Taxes
) : ICommand<IdResponse<Market>>;
