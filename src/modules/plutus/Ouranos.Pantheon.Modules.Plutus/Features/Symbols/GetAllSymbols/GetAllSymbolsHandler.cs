using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;

public sealed class GetAllSymbolsHandler
    : QueryHandler<GetAllSymbolsInput, WrapperResponse<IQueryable<Symbol>>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllSymbolsHandler> _logger;

    public GetAllSymbolsHandler(
        ILogger<GetAllSymbolsHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<Symbol>>> Handle(
        GetAllSymbolsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all symbols query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryable = _dbContext.Symbols.AsQueryable();
        var response = new WrapperResponse<IQueryable<Symbol>>(queryable);

        _logger.LogDebug("Successfully handled get all symbols request.");
        return await Task.FromResult(response);
    }
}
