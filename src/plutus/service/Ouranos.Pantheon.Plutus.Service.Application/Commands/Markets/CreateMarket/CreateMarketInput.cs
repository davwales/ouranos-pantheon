using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Markets.CreateMarket;

public sealed record CreateMarketInput(
    string Name,
    Taxes Taxes
) : ICommand<IdResponse<Market>>;