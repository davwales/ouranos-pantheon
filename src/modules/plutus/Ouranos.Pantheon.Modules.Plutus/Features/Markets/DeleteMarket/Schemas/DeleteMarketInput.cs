using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;

public sealed record DeleteMarketInput(
    Id<Market> MarketId
) : ICommand<IdResponse<Market>>;
