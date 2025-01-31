using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;

public sealed class InsertTrade : IInsertTrade
{
    private readonly ICreateDatabaseId<Trade> _createDatabaseId;
    private readonly ILogger<InsertTrade> _logger;
    private readonly ICrudRepository<Trade> _tradeRepository;

    public InsertTrade(
        ILogger<InsertTrade> logger,
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

    public async Task<Trade> InsertTradeAsync(
        InsertTradeInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to insert trade with input '{@input}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var trade = new Trade(
            _createDatabaseId.CreateId(),
            input.Price,
            input.Volume,
            new TradeMetadata(
                input.MarketId,
                input.SymbolId,
                input.SymbolName,
                input.SymbolCode,
                input.SymbolSubcode,
                input.AdditionalFields,
                input.MessageId
            ),
            input.Timestamp
        );

        await _tradeRepository.Create(trade, cancellationToken);

        _logger.LogDebug("Successfully insert trade '{tradeId}'.", trade.Id);
        return trade;
    }
}