using MediatR;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Service.Plutus.Domain.Markets;

namespace Talos.Olympus.Service.Plutus.Application.Commands.Markets.CreateMarket;

public sealed record CreateMarketInput(
    string Name,
    Taxes Taxes
) : IRequest<IdResponse<Market>>;