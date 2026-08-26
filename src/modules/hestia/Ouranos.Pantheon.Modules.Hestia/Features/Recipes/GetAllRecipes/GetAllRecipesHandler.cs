using Ardalis.GuardClauses;
using Marten;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes;

public sealed class GetAllRecipesHandler(
    ILogger<GetAllRecipesHandler> logger,
    IHestiaMartenStore store,
    IOptions<QueryOptions> queryOptions
) : IPantheonHandler<GetAllRecipesInput, PagedResponse<GetAllRecipesResponse>>
{
    private static readonly FilterBuilder<Recipe> FilterBuilder = new FilterBuilder<Recipe>()
        .On(nameof(Recipe.Title), r => r.Title, caseInsensitive: true)
        .On(nameof(Recipe.SourceUrl), r => r.SourceUrl, caseInsensitive: true);

    private static readonly SortBuilder<Recipe> SortBuilder = new SortBuilder<Recipe>()
        .On(nameof(GetAllRecipesResponse.Title), r => r.Title)
        .Default(r => r.CreatedAt, SortDirection.Desc);

    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);
    private readonly ILogger<GetAllRecipesHandler> _logger = Guard.Against.Null(logger);
    private readonly IOptions<QueryOptions> _queryOptions = Guard.Against.Null(queryOptions);

    public async Task<PagedResponse<GetAllRecipesResponse>> Handle(
        GetAllRecipesInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all recipes query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        using var session = _store.QuerySession();
        var filtered = session.Query<Recipe>().FilterBy(input.Filter, FilterBuilder);
        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await filtered
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .Select(r => new GetAllRecipesResponse(r.RecipeId, r.Title, r.SourceUrl))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all recipes request.");
        return new PagedResponse<GetAllRecipesResponse>(items, totalCount, input.Skip, input.Take);
    }
}
