using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.UpdateMarket;

public sealed record UpdateMarketInput(
    Id<Market> MarketId,
    string Name,
    Taxes Taxes
) : ICommand<IdResponse<Market>>;