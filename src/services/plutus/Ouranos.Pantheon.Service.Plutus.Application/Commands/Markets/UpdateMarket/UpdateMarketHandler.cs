using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.UpdateMarket;

public sealed class UpdateMarketHandler : CommandHandler<UpdateMarketInput, IdResponse<Market>>
{
    private readonly ILogger<UpdateMarketHandler> _logger;
    private readonly ICrudRepository<Market> _marketRepository;

    public UpdateMarketHandler(ILogger<UpdateMarketHandler> logger, ICrudRepository<Market> marketRepository)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(marketRepository);

        _logger = logger;
        _marketRepository = marketRepository;
    }

    protected override async Task<IdResponse<Market>> Handle(
        UpdateMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Attempting to handle update market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _marketRepository.Read(command.MarketId, cancellationToken);
        market.Update(command.Name, command.Taxes);
        await _marketRepository.Update(market, cancellationToken);
        var response = new IdResponse<Market>(market.Id);

        _logger.LogDebug("Successfully handled updated market request.");
        return response;
    }
}