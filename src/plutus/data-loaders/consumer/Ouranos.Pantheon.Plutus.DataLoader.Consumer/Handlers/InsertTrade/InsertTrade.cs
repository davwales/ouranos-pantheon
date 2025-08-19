using Ardalis.GuardClauses;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.DataLoader.Consumer.Models;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.InsertTrade;

public sealed class InsertTrade : IInsertTrade
{
    private readonly ILogger<InsertTrade> _logger;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public InsertTrade(
        ILogger<InsertTrade> logger,
        IPlutusUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Trade> InsertTradeAsync(
        InsertTradeInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to insert trade with input '{@input}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var trade = Trade.Create(
            _unitOfWork.Trades.CreateId(),
            input.Symbol,
            input.Price,
            input.Volume,
            input.Timestamp
        );

        var shouldInsertTrade = await ShouldInsertTrade(trade.Id, input.MessageId, cancellationToken);
        if (!shouldInsertTrade)
        {
            return trade;
        }

        await _unitOfWork.Trades.Create(trade, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

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
                _unitOfWork.TradeMessages.CreateId(),
                tradeId,
                messageId.Value
            );

            await _unitOfWork.TradeMessages.Create(tradeMessage, cancellationToken);
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