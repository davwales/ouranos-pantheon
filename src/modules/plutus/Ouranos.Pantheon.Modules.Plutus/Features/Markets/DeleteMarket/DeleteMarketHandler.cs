using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;

public sealed class DeleteMarketHandler : IPantheonHandler<DeleteMarketInput, IdResponse<Market>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<DeleteMarketHandler> _logger;
    private readonly IMemoryCache _memoryCache;

    public DeleteMarketHandler(
        ILogger<DeleteMarketHandler> logger,
        PlutusDbContext dbContext,
        IMemoryCache memoryCache
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(memoryCache);

        _logger = logger;
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task<IdResponse<Market>> Handle(
        DeleteMarketInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete market command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _dbContext.Markets.FirstOrDefaultAsync(
            m => m.Id == command.MarketId,
            cancellationToken
        );

        Guard.Against.NotFound(command.MarketId, market);

        // Symbols cascade into hundreds of millions of rows across compressed trade, signal and
        // forecast chunks: far too much for one request transaction.
        var hasSymbols = await _dbContext.Symbols.AnyAsync(
            s => s.MarketId == command.MarketId,
            cancellationToken
        );

        if (hasSymbols)
        {
            throw new ConflictException(
                $"The market '{market.Name}' still has symbols and cannot be deleted."
            );
        }

        _dbContext.Markets.Remove(market);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // TradeConsumer caches markets by id; evict so it stops accepting trades for this market.
        _memoryCache.Remove(command.MarketId);

        var response = new IdResponse<Market>(command.MarketId);

        _logger.LogDebug("Successfully handled delete market request.");
        return response;
    }
}
