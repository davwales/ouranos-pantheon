using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;

public sealed class GetRecipeHandler : QueryHandler<GetRecipeInput, Recipe>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetRecipeHandler> _logger;

    public GetRecipeHandler(
        ILogger<GetRecipeHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<Recipe> Handle(
        GetRecipeInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recipe query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var recipe = await _dbContext.Recipes
            .FirstOrDefaultAsync(r => r.Id == query.RecipeId, cancellationToken);

        Guard.Against.NotFound(query.RecipeId, recipe);

        _logger.LogDebug("Successfully handled get recipe request.");
        return recipe;
    }
}
