using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
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

        var query = _symbolRepository.AsQueryable(cancellationToken)
            .Where(s => s.MarketId == request.MarketId && s.Code == request.SymbolCode);
        var existingSymbol = await _queryExecutor.FirstOrDefaultAsync<Symbol?>(query, cancellationToken);

        var symbol = new Symbol(
            existingSymbol?.Id ?? _createDatabaseId.CreateId(),
            request.SymbolCode,
            request.SymbolSubCode,
            request.SymbolName,
            request.MarketId,
            request.AdditionalFields
        );

        await _symbolRepository.Upsert(symbol, cancellationToken);

        _logger.LogDebug("Successfully handled upsert symbol request.");
        return symbol;
    }
}