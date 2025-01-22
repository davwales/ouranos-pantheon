using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;

public sealed class InsertTradeHandler : CommandHandler<InsertTradeInput, Trade>
{
    private readonly ICreateDatabaseId<Trade> _createDatabaseId;
    private readonly ILogger<InsertTradeHandler> _logger;
    private readonly ICrudRepository<Trade> _tradeRepository;

    public InsertTradeHandler(
        ILogger<InsertTradeHandler> logger,
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
        InsertTradeInput command,
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
                command.AdditionalFields,
                command.MessageId
            ),
            command.Timestamp
        );

        await _tradeRepository.Create(trade, cancellationToken);

        _logger.LogDebug("Successfully handled insert trade command.");
        return trade;
    }
}