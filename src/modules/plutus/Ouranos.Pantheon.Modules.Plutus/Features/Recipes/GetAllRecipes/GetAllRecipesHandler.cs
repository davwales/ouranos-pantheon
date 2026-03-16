using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;

public sealed class GetAllRecipesHandler
    : QueryHandler<GetAllRecipesInput, WrapperResponse<IQueryable<Recipe>>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllRecipesHandler> _logger;

    public GetAllRecipesHandler(
        ILogger<GetAllRecipesHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<Recipe>>> Handle(
        GetAllRecipesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all recipes query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryable = _dbContext.Recipes.AsQueryable();
        var response = new WrapperResponse<IQueryable<Recipe>>(queryable);

        _logger.LogDebug("Successfully handled get all recipes request.");
        return await Task.FromResult(response);
    }
}
