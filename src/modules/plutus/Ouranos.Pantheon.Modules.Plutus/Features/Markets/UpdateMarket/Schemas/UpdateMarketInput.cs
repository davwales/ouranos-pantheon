using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;

public sealed record UpdateMarketInput(
    Id<Market> MarketId,
    string Name,
    Taxes Taxes
) : ICommand<IdResponse<Market>>;
