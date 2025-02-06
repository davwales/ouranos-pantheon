using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.CreateMarket;

public sealed class CreateMarketHandler : CommandHandler<CreateMarketInput, IdResponse<Market>>
{
    private readonly ILogger<CreateMarketHandler> _logger;
    private readonly IRepository<Market> _marketRepository;

    public CreateMarketHandler(
        ILogger<CreateMarketHandler> logger,
        IRepository<Market> marketRepository)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(marketRepository);

        _logger = logger;
        _marketRepository = marketRepository;
    }

    public override async Task<IdResponse<Market>> Handle(
        CreateMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Attempting to handle create market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var marketId = _marketRepository.CreateId();
        var market = new Market(marketId, command.Name, command.Taxes);
        await _marketRepository.Create(market, cancellationToken);
        var response = new IdResponse<Market>(marketId);

        _logger.LogDebug("Successfully handle create market request for market '{marketId}'.", marketId);
        return response;
    }
}