using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Common.AsyncLocks;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Messages;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer;

public sealed class UpsertSymbolConsumer : CommandHandler<UpsertSymbolMessage, Symbol>
{
    private readonly ICreateDatabaseId<Symbol> _createDatabaseId;
    private readonly IKeyedAsyncLock<string> _itemLock;
    private readonly ILogger<UpsertSymbolConsumer> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICrudRepository<Symbol> _symbolRepository;

    public UpsertSymbolConsumer(
        ILogger<UpsertSymbolConsumer> logger,
        IQueryExecutor queryExecutor,
        ICrudRepository<Symbol> symbolRepository,
        ICreateDatabaseId<Symbol> createDatabaseId,
        IKeyedAsyncLock<string> itemLock
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(queryExecutor);
        Guard.Against.Null(symbolRepository);
        Guard.Against.Null(createDatabaseId);
        Guard.Against.Null(itemLock);

        _logger = logger;
        _queryExecutor = queryExecutor;
        _symbolRepository = symbolRepository;
        _createDatabaseId = createDatabaseId;
        _itemLock = itemLock;
    }

    protected override async Task<Symbol> Handle(
        UpsertSymbolMessage command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle upsert symbol command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        // If we allow multiple messages for the same symbol to be processed concurrently, we run the risk of 
        // multiple symbols being inserted. Thus, we lock the upsert for messages that have the same symbol.
        var lockKey = $"{nameof(TradeConsumer)}:{command.MarketId}:{command.SymbolCode}";
        using var upsertLock = await _itemLock.LockAsync(lockKey);

        var existingSymbol = await _queryExecutor.FirstOrDefaultAsync<Symbol?>(
            GetSymbolQuery(command.MarketId, command.SymbolCode, command.SymbolSubcode),
            cancellationToken
        );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(command.SymbolName, command.AdditionalFields);
            await _symbolRepository.Update(existingSymbol, cancellationToken);
            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            return existingSymbol;
        }

        var newSymbol = new Symbol(
            _createDatabaseId.CreateId(),
            command.SymbolCode,
            command.SymbolSubcode,
            command.SymbolName,
            command.MarketId,
            command.AdditionalFields
        );

        await _symbolRepository.Create(newSymbol, cancellationToken);

        _logger.LogDebug("Successfully inserted new symbol '{symbolId}'.", newSymbol.Id);
        return newSymbol;
    }

    private IQueryable<Symbol> GetSymbolQuery(Id<Market> marketId, string symbolCode, string? symbolSubcode)
    {
        return _symbolRepository.AsQueryable()
            .Where(s => s.MarketId == marketId && s.Code == symbolCode && s.Subcode == symbolSubcode);
    }
}