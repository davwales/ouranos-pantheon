using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;

public sealed class GetAllRecipesHandler
    : IPantheonHandler<GetAllRecipesInput, PagedResponse<GetAllRecipesResponse>>
{
    private static readonly FilterBuilder<Recipe> FilterBuilder = new FilterBuilder<Recipe>()
        .On(nameof(Recipe.MarketId), r => r.MarketId)
        .On(nameof(Recipe.Name), r => r.Name, caseInsensitive: true)
        .On(nameof(Recipe.Cost), r => r.Cost);

    private static readonly SortBuilder<Recipe> SortBuilder = new SortBuilder<Recipe>()
        .On(nameof(GetAllRecipesResponse.Name), r => r.Name)
        .Default(r => r.Name, SortDirection.Asc);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllRecipesHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllRecipesHandler(
        ILogger<GetAllRecipesHandler> logger,
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

    public async Task<PagedResponse<GetAllRecipesResponse>> Handle(
        GetAllRecipesInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all recipes query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(input.Take, nameof(input.Take), limits.MinPageSize, limits.MaxPageSize);

        var q = _dbContext.Recipes
            .AsQueryable()
            .AsNoTracking()
            .FilterBy(input.Filter, FilterBuilder);

        var totalCount = await q.CountAsync(cancellationToken);

        var items = await q
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .Select(r => new GetAllRecipesResponse(r.Id, r.MarketId, r.Name, r.Cost))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all recipes request.");
        return new PagedResponse<GetAllRecipesResponse>(items, totalCount, input.Skip, input.Take);
    }
}
