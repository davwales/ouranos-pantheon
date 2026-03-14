using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Recipes.CreateRecipe;

public sealed class CreateRecipeHandler : CommandHandler<CreateRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<CreateRecipeHandler> _logger;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public CreateRecipeHandler(
        ILogger<CreateRecipeHandler> logger,
        IPlutusUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override async Task<IdResponse<Recipe>> Handle(
        CreateRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var market = await _unitOfWork.Markets.Read(command.MarketId, cancellationToken);

        var recipe = Recipe.Create(
            _unitOfWork.Recipes.CreateId(),
            market,
            command.Name,
            command.Cost,
            command.Inputs,
            command.Outputs
        );

        await _unitOfWork.Recipes.Create(recipe, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        var response = new IdResponse<Recipe>(recipe.Id);

        _logger.LogDebug("Successfully handled create recipe command.");
        return response;
    }
}