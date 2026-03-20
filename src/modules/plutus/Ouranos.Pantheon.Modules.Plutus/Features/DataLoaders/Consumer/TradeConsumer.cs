using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;

public sealed class TradeConsumer : IPantheonHandler<TradeMessage>
{
    private readonly ILogger<TradeConsumer> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly IOptions<ConsumerDataLoaderOptions> _consumerDataLoaderOptions;
    private readonly IMemoryCache _memoryCache;

    public TradeConsumer(
        ILogger<TradeConsumer> logger,
        IOptions<ConsumerDataLoaderOptions> consumerDataLoaderOptions,
        PlutusDbContext dbContext,
        IMemoryCache memoryCache
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(consumerDataLoaderOptions);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(memoryCache);

        _logger = logger;
        _consumerDataLoaderOptions = consumerDataLoaderOptions;
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task Handle(TradeMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to consume trade message for symbol '{symbolCode}'.", message.SymbolCode);

        var trade = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            await UpsertSymbol(message, cancellationToken),
            message.Price,
            message.Volume,
            message.Timestamp
        );

        await _dbContext.Trades.AddAsync(trade, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully consumed trade message for symbol '{symbolCode}'.", message.SymbolCode);
    }

    private async Task<Symbol> UpsertSymbol(
        TradeMessage message,
        CancellationToken cancellationToken
    )
    {
        var marketIdStr = _consumerDataLoaderOptions.Value.MarketMap.GetValueOrDefault(message.Producer);
        Guard.Against.NotFound(message.Producer, marketIdStr);

        var marketId = new Id<Market>(marketIdStr);
        var market = await GetMarket(marketId, cancellationToken);

        var existingSymbol = await _dbContext.Symbols
            .FirstOrDefaultAsync(
                s => s.MarketId == marketId &&
                     s.Code == message.SymbolCode &&
                     s.Subcode == message.SymbolSubcode,
                cancellationToken
            );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(message.SymbolName, message.AdditionalFields);
            _dbContext.Symbols.Update(existingSymbol);

            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            return existingSymbol;
        }

        var newSymbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            message.SymbolCode,
            message.SymbolSubcode,
            message.SymbolName,
            market,
            message.AdditionalFields
        );

        await _dbContext.Symbols.AddAsync(newSymbol, cancellationToken);
        return newSymbol;
    }

    private async Task<Market> GetMarket(
        Id<Market> marketId,
        CancellationToken cancellationToken
    )
    {
        if (_memoryCache.TryGetValue(marketId, out Market? cached) && cached is not null)
        {
            _dbContext.Markets.Attach(cached);
            return cached;
        }

        var market = await _dbContext.Markets
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == marketId, cancellationToken);

        Guard.Against.NotFound(marketId, market);

        _memoryCache.Set(marketId, market);
        return market;
    }
}
