using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.InsertTrade;

public sealed class InsertTrade : IInsertTrade
{
    private readonly ILogger<InsertTrade> _logger;
    private readonly PlutusDbContext _dbContext;

    public InsertTrade(
        ILogger<InsertTrade> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Trade> InsertTradeAsync(
        InsertTradeInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to insert trade with input '{@input}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var trade = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
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

        await _dbContext.Trades.AddAsync(trade, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

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

        var tradeMessage = TradeMessage.Create(
            new Id<TradeMessage>(Guid.NewGuid().ToString()),
            tradeId,
            messageId.Value
        );

        await _dbContext.TradeMessages.AddAsync(tradeMessage, cancellationToken);

        return true;
    }
}