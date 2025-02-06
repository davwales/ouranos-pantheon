using Ardalis.GuardClauses;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Models;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;

public sealed class InsertTrade : IInsertTrade
{
    private readonly ICreateDatabaseId<Trade> _createTradeId;
    private readonly ICreateDatabaseId<TradeMessage> _createTradeMessageId;
    private readonly ILogger<InsertTrade> _logger;
    private readonly ICrudRepository<TradeMessage> _tradeMessageRepository;
    private readonly ICrudRepository<Trade> _tradeRepository;

    public InsertTrade(
        ILogger<InsertTrade> logger,
        ICreateDatabaseId<Trade> createTradeId,
        ICreateDatabaseId<TradeMessage> createTradeMessageId,
        ICrudRepository<TradeMessage> tradeMessageRepository,
        ICrudRepository<Trade> tradeRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(createTradeId);
        Guard.Against.Null(createTradeMessageId);
        Guard.Against.Null(tradeMessageRepository);
        Guard.Against.Null(tradeRepository);

        _logger = logger;
        _createTradeId = createTradeId;
        _createTradeMessageId = createTradeMessageId;
        _tradeMessageRepository = tradeMessageRepository;
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
            _createTradeId.CreateId(),
            input.Price,
            input.Volume,
            new TradeMetadata(
                input.MarketId,
                input.SymbolId,
                input.SymbolName,
                input.SymbolCode,
                input.SymbolSubcode,
                input.AdditionalFields
            ),
            input.Timestamp
        );

        var shouldInsertTrade = await ShouldInsertTrade(trade.Id, input.MessageId, cancellationToken);
        if (!shouldInsertTrade)
        {
            return trade;
        }

        await _tradeRepository.Create(trade, cancellationToken);

        _logger.LogDebug("Successfully insert trade '{tradeId}'.", trade.Id);
        return trade;
    }

    private async Task<bool> ShouldInsertTrade(
        Id<Trade> tradeId,
        Guid? messageId,
        CancellationToken cancellationToken
    )
    {
        if (!messageId.HasValue)
        {
            return true;
        }

        try
        {
            var tradeMessage = new TradeMessage(
                _createTradeMessageId.CreateId(),
                tradeId,
                messageId.Value
            );

            await _tradeMessageRepository.Create(tradeMessage, cancellationToken);
        }
        catch (MongoDuplicateKeyException)
        {
            _logger.LogWarning("Detected duplicate trade message '{messageId}', ignoring.'", messageId);
            return false;
        }
        catch (MongoWriteException writeException)
        {
            if (writeException.WriteError.Category != ServerErrorCategory.DuplicateKey)
            {
                throw;
            }

            _logger.LogWarning("Detected duplicate trade message '{messageId}', ignoring.", messageId);
            return false;
        }

        return true;
    }
}