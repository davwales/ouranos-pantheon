using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe;

public sealed class UpdateRecipeHandler : IPantheonHandler<UpdateRecipeInput, IdResponse<Recipe>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<UpdateRecipeHandler> _logger;

    public UpdateRecipeHandler(
        ILogger<UpdateRecipeHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Recipe>> Handle(
        UpdateRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _dbContext.Markets
            .FirstOrDefaultAsync(m => m.Id == command.MarketId, cancellationToken);

        Guard.Against.NotFound(command.MarketId, market);

        var recipe = await _dbContext.Recipes
            .FirstOrDefaultAsync(r => r.Id == command.RecipeId, cancellationToken);

        Guard.Against.NotFound(command.RecipeId, recipe);

        _dbContext.Recipes.Remove(recipe);

        var updatedRecipe = Recipe.Create(
            command.RecipeId,
            market,
            command.Name,
            command.Cost,
            command.Inputs,
            command.Outputs
        );

        await _dbContext.Recipes.AddAsync(updatedRecipe, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var response = new IdResponse<Recipe>(updatedRecipe.Id);

        _logger.LogDebug("Successfully handled update recipe command.");
        return response;
    }
}
