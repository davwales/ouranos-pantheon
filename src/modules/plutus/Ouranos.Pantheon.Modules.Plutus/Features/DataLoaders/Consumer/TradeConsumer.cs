using Ardalis.GuardClauses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;

public sealed class TradeConsumer : IConsumer<TradeMessage>
{
    private readonly ILogger<TradeConsumer> _logger;
    private readonly PlutusDbContext _dbContext;

    private readonly IOptions<ConsumerDataLoaderOptions> _consumerDataLoaderOptions;

    public TradeConsumer(
        ILogger<TradeConsumer> logger,
        IOptions<ConsumerDataLoaderOptions> consumerDataLoaderOptions,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(consumerDataLoaderOptions);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _consumerDataLoaderOptions = consumerDataLoaderOptions;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<TradeMessage> context)
    {
        _logger.LogTrace("Attempting to consume trade message '{messageId}'.", context.MessageId);

        var trade = Trade.Create(
            new Id<Trade>((context.MessageId ?? Guid.NewGuid()).ToString()),
            await UpsertSymbol(context, context.CancellationToken),
            context.Message.Price,
            context.Message.Volume,
            context.Message.Timestamp
        );

        await _dbContext.Trades.AddAsync(trade, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Successfully consumed trade message '{messageId}'.", context.MessageId);
    }

    private async Task<Symbol> UpsertSymbol(
        ConsumeContext<TradeMessage> context,
        CancellationToken cancellationToken
    )
    {
        var marketIdStr = _consumerDataLoaderOptions.Value.MarketMap.GetValueOrDefault(context.Message.Producer);
        Guard.Against.NotFound(context.Message.Producer, marketIdStr);

        var marketId = new Id<Market>(marketIdStr);

        var market = await _dbContext.Markets.FirstOrDefaultAsync(m => m.Id == marketId, cancellationToken);
        Guard.Against.NotFound(marketId, market);

        var existingSymbol = await _dbContext.Symbols
            .FirstOrDefaultAsync(
                s => s.MarketId == marketId &&
                     s.Code == context.Message.SymbolCode &&
                     s.Subcode == context.Message.SymbolSubcode,
                cancellationToken
            );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(context.Message.SymbolName, context.Message.AdditionalFields);
            _dbContext.Symbols.Update(existingSymbol);

            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            return existingSymbol;
        }

        var newSymbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            context.Message.SymbolCode,
            context.Message.SymbolSubcode,
            context.Message.SymbolName,
            market,
            context.Message.AdditionalFields
        );

        await _dbContext.Symbols.AddAsync(newSymbol, cancellationToken);
        return newSymbol;
    }
}
