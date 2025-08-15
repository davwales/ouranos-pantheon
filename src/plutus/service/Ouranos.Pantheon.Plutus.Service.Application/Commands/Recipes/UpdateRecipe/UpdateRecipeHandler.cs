using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Recipes.UpdateRecipe;

public sealed class UpdateRecipeHandler : CommandHandler<UpdateRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<UpdateRecipeHandler> _logger;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public UpdateRecipeHandler(
        ILogger<UpdateRecipeHandler> logger,
        IPlutusUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override async Task<IdResponse<Recipe>> Handle(
        UpdateRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var updatedRecipe = new Recipe(
            command.RecipeId,
            command.MarketId,
            command.Name,
            command.Cost,
            command.Inputs,
            command.Outputs
        );

        await _unitOfWork.Recipes.Update(updatedRecipe, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        var response = new IdResponse<Recipe>(updatedRecipe.Id);

        _logger.LogDebug("Successfully handled update recipe command.");
        return response;
    }
}