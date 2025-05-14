using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Recipes.CreateRecipe;

public sealed class CreateRecipeHandler : CommandHandler<CreateRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<CreateRecipeHandler> _logger;
    private readonly IRepository<Recipe> _recipeRepository;

    public CreateRecipeHandler(
        ILogger<CreateRecipeHandler> logger,
        IRepository<Recipe> recipeRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(recipeRepository);

        _logger = logger;
        _recipeRepository = recipeRepository;
    }

    public override async Task<IdResponse<Recipe>> Handle(
        CreateRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var recipe = new Recipe(
            _recipeRepository.CreateId(),
            command.MarketId,
            command.Name,
            command.Cost,
            command.Inputs,
            command.Outputs
        );

        await _recipeRepository.Create(recipe, cancellationToken);
        var response = new IdResponse<Recipe>(recipe.Id);

        _logger.LogDebug("Successfully handled create recipe command.");
        return response;
    }
}