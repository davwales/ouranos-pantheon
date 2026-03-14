using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Markets.UpdateMarket;

public sealed record UpdateMarketInput(
    Id<Market> MarketId,
    string Name,
    Taxes Taxes
) : ICommand<IdResponse<Market>>;