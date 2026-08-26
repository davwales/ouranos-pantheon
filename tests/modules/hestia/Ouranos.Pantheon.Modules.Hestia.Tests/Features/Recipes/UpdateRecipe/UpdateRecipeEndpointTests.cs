using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.UpdateRecipe;

public sealed class UpdateRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithUpdatedRecipeId()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid().ToString();
        var recipeId = new Id<Recipe>(id);
        var body = new UpdateRecipeBody(
            "Updated Title",
            null,
            [new StepInput("Mix and bake.")],
            [new IngredientInput(0m, "cups", "flour")],
            "Warm."
        );
        var expected = new IdResponse<Recipe>(recipeId);

        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateRecipeEndpoint.Handle(recipeId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Recipe>>>();
        await _bus.Received(1)
            .InvokeAsync<IdResponse<Recipe>>(
                Arg.Is<UpdateRecipeInput>(input => input.RecipeId == recipeId),
                ct
            );
    }
}
