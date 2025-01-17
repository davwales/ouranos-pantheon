using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.CreateMarket;

public sealed class CreateMarketHandler : ICommandHandler<CreateMarketInput, IdResponse<Market>>
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

    public async Task Consume(ConsumeContext<CreateMarketInput> context)
    {
        _logger.LogDebug("Attempting to handle create market command '{@command}'.", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        var marketId = _createDatabaseId.CreateId();
        var market = new Market(marketId, context.Message.Name, context.Message.Taxes);
        await _marketRepository.Create(market, context.CancellationToken);

        _logger.LogDebug("Successfully handle create market request for market '{marketId}'.", marketId);
        await context.RespondAsync(new IdResponse<Market>(marketId));
    }
}