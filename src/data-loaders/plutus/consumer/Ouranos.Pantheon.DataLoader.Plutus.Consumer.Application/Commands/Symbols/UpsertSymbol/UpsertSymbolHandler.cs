using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application.Commands.Symbols.UpsertSymbol;

public sealed class UpsertSymbolHandler : IRequestHandler<UpsertSymbolInput, Symbol>
{
    private readonly ICreateDatabaseId<Symbol> _createDatabaseId;
    private readonly ILogger<UpsertSymbolHandler> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICrudRepository<Symbol> _symbolRepository;

    public UpsertSymbolHandler(
        ILogger<UpsertSymbolHandler> logger,
        IQueryExecutor queryExecutor,
        ICrudRepository<Symbol> symbolRepository,
        ICreateDatabaseId<Symbol> createDatabaseId
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(queryExecutor);
        ArgumentNullException.ThrowIfNull(symbolRepository);
        ArgumentNullException.ThrowIfNull(createDatabaseId);

        _logger = logger;
        _queryExecutor = queryExecutor;
        _symbolRepository = symbolRepository;
        _createDatabaseId = createDatabaseId;
    }

    public async Task<Symbol> Handle(
        UpsertSymbolInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle upsert symbol request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        var existingSymbol = await _queryExecutor.FirstOrDefaultAsync<Symbol?>(
            GetSymbolQuery(request.MarketId, request.SymbolCode, request.SymbolSubcode),
            cancellationToken
        );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(request.SymbolName, request.AdditionalFields);
            await _symbolRepository.Update(existingSymbol, cancellationToken);
            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            return existingSymbol;
        }

        var newSymbol = new Symbol(
            _createDatabaseId.CreateId(),
            request.SymbolCode,
            request.SymbolSubcode,
            request.SymbolName,
            request.MarketId,
            request.AdditionalFields
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