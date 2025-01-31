using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Common.AsyncLocks;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.UpsertSymbol;

public sealed class UpsertSymbol : IUpsertSymbol
{
    private readonly ICreateDatabaseId<Symbol> _createDatabaseId;
    private readonly IKeyedAsyncLock<string> _itemLock;
    private readonly ILogger<UpsertSymbol> _logger;
    private readonly ICrudRepository<Symbol> _symbolRepository;

    public UpsertSymbol(
        ILogger<UpsertSymbol> logger,
        ICrudRepository<Symbol> symbolRepository,
        ICreateDatabaseId<Symbol> createDatabaseId,
        IKeyedAsyncLock<string> itemLock
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(symbolRepository);
        Guard.Against.Null(createDatabaseId);
        Guard.Against.Null(itemLock);

        _logger = logger;
        _symbolRepository = symbolRepository;
        _createDatabaseId = createDatabaseId;
        _itemLock = itemLock;
    }

    public async Task<Symbol> UpsertSymbolAsync(
        UpsertSymbolInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to upsert symbol with input '{@input}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        // If we allow multiple messages for the same symbol to be processed concurrently, we run the risk of 
        // multiple symbols being inserted. Thus, we lock the upsert for messages that have the same symbol.
        var lockKey = $"{input.MarketId}:{input.SymbolCode}:{input.SymbolSubcode}";
        using var upsertLock = await _itemLock.LockAsync(lockKey);

        var existingSymbol = await _symbolRepository.FirstOrDefault(
            s => s.MarketId == input.MarketId &&
                 s.Code == input.SymbolCode &&
                 s.Subcode == input.SymbolSubcode,
            cancellationToken
        );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(input.SymbolName, input.AdditionalFields);
            await _symbolRepository.Update(existingSymbol, cancellationToken);
            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            return existingSymbol;
        }

        var newSymbol = new Symbol(
            _createDatabaseId.CreateId(),
            input.SymbolCode,
            input.SymbolSubcode,
            input.SymbolName,
            input.MarketId,
            input.AdditionalFields
        );

        await _symbolRepository.Create(newSymbol, cancellationToken);

        _logger.LogDebug("Successfully inserted new symbol '{symbolId}'.", newSymbol.Id);
        return newSymbol;
    }
}