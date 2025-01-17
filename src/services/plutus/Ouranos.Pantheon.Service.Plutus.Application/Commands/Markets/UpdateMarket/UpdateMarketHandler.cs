using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.UpdateMarket;

public sealed class UpdateMarketHandler : ICommandHandler<UpdateMarketInput, IdResponse<Market>>
{
    private readonly ILogger<UpdateMarketHandler> _logger;
    private readonly ICrudRepository<Market> _marketRepository;

    public UpdateMarketHandler(ILogger<UpdateMarketHandler> logger, ICrudRepository<Market> marketRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(marketRepository);

        _logger = logger;
        _marketRepository = marketRepository;
    }

    public async Task Consume(ConsumeContext<UpdateMarketInput> context)
    {
        _logger.LogDebug("Attempting to handle update market command '{@command}'.", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        var market = await _marketRepository.Read(context.Message.MarketId, context.CancellationToken);
        market.Update(context.Message.Name, context.Message.Taxes);
        await _marketRepository.Update(market, context.CancellationToken);

        _logger.LogDebug("Successfully handled updated market request.");
        await context.RespondAsync(new IdResponse<Market>(market.Id));
    }
}