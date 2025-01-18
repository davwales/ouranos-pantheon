using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.CreateMarket;

public sealed class CreateMarketHandler : CommandHandler<CreateMarketInput, IdResponse<Market>>
{
    private readonly ICreateDatabaseId<Market> _createDatabaseId;
    private readonly ILogger<CreateMarketHandler> _logger;
    private readonly ICrudRepository<Market> _marketRepository;

    public CreateMarketHandler(
        ILogger<CreateMarketHandler> logger,
        ICreateDatabaseId<Market> createDatabaseId,
        ICrudRepository<Market> marketRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(createDatabaseId);
        ArgumentNullException.ThrowIfNull(marketRepository);

        _logger = logger;
        _createDatabaseId = createDatabaseId;
        _marketRepository = marketRepository;
    }

    protected override async Task<IdResponse<Market>> Handle(
        CreateMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Attempting to handle create market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var marketId = _createDatabaseId.CreateId();
        var market = new Market(marketId, command.Name, command.Taxes);
        await _marketRepository.Create(market, cancellationToken);
        var response = new IdResponse<Market>(marketId);

        _logger.LogDebug("Successfully handle create market request for market '{marketId}'.", marketId);
        return response;
    }
}