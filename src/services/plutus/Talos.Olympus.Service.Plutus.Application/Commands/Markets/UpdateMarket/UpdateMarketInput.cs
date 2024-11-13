using MediatR;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Domain.Markets;

namespace Talos.Olympus.Service.Plutus.Application.Commands.Markets.UpdateMarket;

public sealed record UpdateMarketInput(
    Id<Market> MarketId,
    string Name,
    Taxes Taxes
) : IRequest<IdResponse<Market>>;