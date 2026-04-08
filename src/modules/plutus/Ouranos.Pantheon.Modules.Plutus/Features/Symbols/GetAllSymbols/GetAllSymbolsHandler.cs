using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;

public sealed class GetAllSymbolsHandler
    : IPantheonHandler<GetAllSymbolsInput, PagedResponse<GetAllSymbolsResponse>>
{
    private static readonly FilterBuilder<Symbol> FilterBuilder = new FilterBuilder<Symbol>()
        .On(nameof(Symbol.MarketId), s => s.MarketId)
        .On(nameof(Symbol.Name), s => s.Name, caseInsensitive: true)
        .On(nameof(Symbol.Code), s => s.Code, caseInsensitive: true)
        .On(nameof(Symbol.Subcode), s => s.Subcode, caseInsensitive: true);

    private static readonly SortBuilder<Symbol> SortBuilder = new SortBuilder<Symbol>()
        .On(nameof(GetAllSymbolsResponse.Name), s => s.Name)
        .Default(s => s.Name, SortDirection.Asc);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllSymbolsHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllSymbolsHandler(
        ILogger<GetAllSymbolsHandler> logger,
        PlutusDbContext dbContext,
        IOptions<QueryOptions> queryOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(queryOptions);

        _logger = logger;
        _dbContext = dbContext;
        _queryOptions = queryOptions;
    }

    public async Task<PagedResponse<GetAllSymbolsResponse>> Handle(
        GetAllSymbolsInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all symbols query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(input.Take, nameof(input.Take), limits.MinPageSize, limits.MaxPageSize);

        var q = _dbContext.Symbols
            .AsQueryable()
            .AsNoTracking()
            .FilterBy(input.Filter, FilterBuilder);

        var totalCount = await q.CountAsync(cancellationToken);

        var items = await q
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .Select(s => new GetAllSymbolsResponse(s.Id, s.Code, s.Subcode, s.Name, s.MarketId))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all symbols request.");
        return new PagedResponse<GetAllSymbolsResponse>(items, totalCount, input.Skip, input.Take);
    }
}
