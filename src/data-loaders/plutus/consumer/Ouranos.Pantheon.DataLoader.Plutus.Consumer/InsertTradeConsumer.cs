using MassTransit;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Messages;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer;

public sealed class InsertTradeConsumer : ICommandHandler<InsertTradeMessage, Trade>
{
    private readonly ICreateDatabaseId<Trade> _createDatabaseId;
    private readonly ILogger<InsertTradeConsumer> _logger;
    private readonly ICrudRepository<Trade> _tradeRepository;

    public InsertTradeConsumer(
        ILogger<InsertTradeConsumer> logger,
        ICreateDatabaseId<Trade> createDatabaseId,
        ICrudRepository<Trade> tradeRepository
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(createDatabaseId);
        ArgumentNullException.ThrowIfNull(tradeRepository);

        _logger = logger;
        _createDatabaseId = createDatabaseId;
        _tradeRepository = tradeRepository;
    }

    public async Task Consume(ConsumeContext<InsertTradeMessage> context)
    {
        _logger.LogTrace("Attempting to handle process insert trade command '{@command}'.", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        var trade = new Trade(
            _createDatabaseId.CreateId(),
            context.Message.Price,
            context.Message.Volume,
            new TradeMetadata(
                context.Message.MarketId,
                context.Message.SymbolId,
                context.Message.SymbolName,
                context.Message.SymbolCode,
                context.Message.SymbolSubcode,
                context.Message.AdditionalFields
            ),
            context.Message.Timestamp
        );

        await _tradeRepository.Create(trade, context.CancellationToken);

        _logger.LogDebug("Successfully handled insert trade command.");
        await context.RespondAsync(trade);
    }
}