using MassTransit;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Core.Common.AsyncLocks;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Messages;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer;

public sealed class UpsertSymbolConsumer : ICommandHandler<UpsertSymbolMessage, Symbol>
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
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(queryExecutor);
        ArgumentNullException.ThrowIfNull(symbolRepository);
        ArgumentNullException.ThrowIfNull(createDatabaseId);
        ArgumentNullException.ThrowIfNull(itemLock);

        _logger = logger;
        _queryExecutor = queryExecutor;
        _symbolRepository = symbolRepository;
        _createDatabaseId = createDatabaseId;
        _itemLock = itemLock;
    }

    public async Task Consume(ConsumeContext<UpsertSymbolMessage> context)
    {
        _logger.LogTrace("Attempting to handle upsert symbol command '{@command}'.", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        // If we allow multiple messages for the same symbol to be processed concurrently, we run the risk of 
        // multiple symbols being inserted. Thus, we lock the upsert for messages that have the same symbol.
        var lockKey = $"{nameof(TradeConsumer)}:{context.Message.MarketId}:{context.Message.SymbolCode}";
        using var upsertLock = await _itemLock.LockAsync(lockKey);

        var existingSymbol = await _queryExecutor.FirstOrDefaultAsync<Symbol?>(
            GetSymbolQuery(context.Message.MarketId, context.Message.SymbolCode, context.Message.SymbolSubcode),
            context.CancellationToken
        );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(context.Message.SymbolName, context.Message.AdditionalFields);
            await _symbolRepository.Update(existingSymbol, context.CancellationToken);
            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            await context.RespondAsync(existingSymbol);
            return;
        }

        var newSymbol = new Symbol(
            _createDatabaseId.CreateId(),
            context.Message.SymbolCode,
            context.Message.SymbolSubcode,
            context.Message.SymbolName,
            context.Message.MarketId,
            context.Message.AdditionalFields
        );

        await _symbolRepository.Create(newSymbol, context.CancellationToken);

        _logger.LogDebug("Successfully inserted new symbol '{symbolId}'.", newSymbol.Id);
        await context.RespondAsync(newSymbol);
    }

    private IQueryable<Symbol> GetSymbolQuery(Id<Market> marketId, string symbolCode, string? symbolSubcode)
    {
        return _symbolRepository.AsQueryable()
            .Where(s => s.MarketId == marketId && s.Code == symbolCode && s.Subcode == symbolSubcode);
    }
}