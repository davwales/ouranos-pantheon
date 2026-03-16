using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol;

public sealed class GetSymbolHandler : QueryHandler<GetSymbolInput, Symbol>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetSymbolHandler> _logger;

    public GetSymbolHandler(
        ILogger<GetSymbolHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<Symbol> Handle(
        GetSymbolInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbol query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = await _dbContext.Symbols.FirstOrDefaultAsync(s => s.Id == query.SymbolId, cancellationToken);

        Guard.Against.NotFound(query.SymbolId, symbol);

        _logger.LogDebug("Successfully handled get symbol request.");
        return symbol;
    }
}
