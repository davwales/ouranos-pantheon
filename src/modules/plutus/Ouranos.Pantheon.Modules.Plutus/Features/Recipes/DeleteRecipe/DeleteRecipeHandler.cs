using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe;

public sealed class DeleteRecipeHandler : IPantheonHandler<DeleteRecipeInput, IdResponse<Recipe>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<DeleteRecipeHandler> _logger;

    public DeleteRecipeHandler(ILogger<DeleteRecipeHandler> logger, PlutusDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Recipe>> Handle(
        DeleteRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var recipe = await _dbContext.Recipes.FirstOrDefaultAsync(
            r => r.Id == command.RecipeId,
            cancellationToken
        );

        Guard.Against.NotFound(command.RecipeId, recipe);

        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var response = new IdResponse<Recipe>(command.RecipeId);

        _logger.LogDebug("Successfully handled delete recipe request.");
        return response;
    }
}
