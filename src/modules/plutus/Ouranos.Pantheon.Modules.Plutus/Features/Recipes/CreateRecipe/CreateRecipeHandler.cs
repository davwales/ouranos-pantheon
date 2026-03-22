using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe;

public sealed class CreateRecipeHandler : IPantheonHandler<CreateRecipeInput, IdResponse<Recipe>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<CreateRecipeHandler> _logger;

    public CreateRecipeHandler(
        ILogger<CreateRecipeHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IdResponse<Recipe>> Handle(
        CreateRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var recipe = Recipe.Create(
            DatabaseExtensions.CreateId<Recipe>(),
            command.MarketId,
            command.Name,
            command.Cost,
            command.Inputs,
            command.Outputs
        );

        await _dbContext.Recipes.AddAsync(recipe, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled create recipe command.");
        return new IdResponse<Recipe>(recipe.Id);
    }
}
