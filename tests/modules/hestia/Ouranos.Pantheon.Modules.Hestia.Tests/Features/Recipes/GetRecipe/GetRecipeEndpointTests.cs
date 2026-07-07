using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.GetRecipe;

public sealed class GetRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid().ToString();
        var recipeId = new Id<Recipe>(id);
        var expected = new GetRecipeResponse(
            recipeId,
            "Chocolate Cake",
            "https://example.com/cake",
            [new StepResponse("Mix and bake.")],
            [new IngredientResponse(4m, "tablespoons", "granulated sugar")],
            "Best served warm.",
            DateTimeOffset.UtcNow
        );

        _bus.InvokeAsync<GetRecipeResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetRecipeEndpoint.Handle(recipeId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetRecipeResponse>>();
        await _bus.Received(1)
            .InvokeAsync<GetRecipeResponse>(
                Arg.Is<GetRecipeInput>(input => input.RecipeId == recipeId),
                ct
            );
    }
}
