using Ardalis.GuardClauses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Plutus.DataLoader.Domain;
using Ouranos.Pantheon.Plutus.DataLoader.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer;

public sealed class TradeConsumer : IConsumer<TradeMessage>
{
    private readonly ILogger<TradeConsumer> _logger;
    private readonly Dictionary<Producer, Id<Market>> _marketMap;
    private readonly PlutusDbContext _dbContext;

    public TradeConsumer(
        ILogger<TradeConsumer> logger,
        IConfiguration configuration,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(configuration);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;

        var marketConfig = configuration
            .GetSection("Ouranos:Markets")
            .Get<Dictionary<Producer, string>>();

        Guard.Against.Null(marketConfig);

        _marketMap = marketConfig.ToDictionary(
            x => x.Key,
            x => new Id<Market>(x.Value)
        );
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
        var marketId = _marketMap.GetValueOrDefault(context.Message.Producer);
        Guard.Against.NotFound(context.Message.Producer, marketId);

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