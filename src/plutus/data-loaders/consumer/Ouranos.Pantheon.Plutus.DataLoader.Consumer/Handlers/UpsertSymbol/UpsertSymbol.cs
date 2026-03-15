using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.UpsertSymbol;

public sealed class UpsertSymbol : IUpsertSymbol
{
    private readonly ILogger<UpsertSymbol> _logger;
    private readonly PlutusDbContext _dbContext;

    public UpsertSymbol(
        ILogger<UpsertSymbol> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Symbol> UpsertSymbolAsync(
        UpsertSymbolInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to upsert symbol with input '{@input}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var existingSymbol = await _dbContext.Symbols
            .FirstOrDefaultAsync(
                s => s.MarketId == input.MarketId &&
                     s.Code == input.SymbolCode &&
                     s.Subcode == input.SymbolSubcode,
                cancellationToken
            );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(input.SymbolName, input.AdditionalFields);
            _dbContext.Symbols.Update(existingSymbol);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            return existingSymbol;
        }

        var market = await _dbContext.Markets.FirstOrDefaultAsync(m => m.Id == input.MarketId, cancellationToken);

        if (market is null)
        {
            throw new InvalidOperationException($"Market '{input.MarketId}' not found.");
        }

        var newSymbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            input.SymbolCode,
            input.SymbolSubcode,
            input.SymbolName,
            market,
            input.AdditionalFields
        );

        await _dbContext.Symbols.AddAsync(newSymbol, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully inserted new symbol '{symbolId}'.", newSymbol.Id);
        return newSymbol;
    }
}