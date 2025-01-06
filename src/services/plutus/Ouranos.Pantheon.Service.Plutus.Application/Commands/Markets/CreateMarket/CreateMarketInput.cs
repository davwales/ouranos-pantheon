using MediatR;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.CreateMarket;

public sealed record CreateMarketInput(
    string Name,
    Taxes Taxes
) : IRequest<IdResponse<Market>>;