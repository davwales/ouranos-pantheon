using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket.Schemas;

public sealed record CreateMarketInput(
    string Name,
    Taxes Taxes,
    bool IsForecastingEnabled = false,
    string? Description = null,
    string? Icon = null
) : ICommand<IdResponse<Market>>;
