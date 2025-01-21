using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Messages;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer;

public sealed class InsertTradeConsumer : CommandHandler<InsertTradeMessage, Trade>
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
        Guard.Against.Null(logger);
        Guard.Against.Null(createDatabaseId);
        Guard.Against.Null(tradeRepository);

        _logger = logger;
        _createDatabaseId = createDatabaseId;
        _tradeRepository = tradeRepository;
    }

    protected override async Task<Trade> Handle(
        InsertTradeMessage command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle process insert trade command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var trade = new Trade(
            _createDatabaseId.CreateId(),
            command.Price,
            command.Volume,
            new TradeMetadata(
                command.MarketId,
                command.SymbolId,
                command.SymbolName,
                command.SymbolCode,
                command.SymbolSubcode,
                command.AdditionalFields
            ),
            command.Timestamp
        );

        await _tradeRepository.Create(trade, cancellationToken);

        _logger.LogDebug("Successfully handled insert trade command.");
        return trade;
    }
}