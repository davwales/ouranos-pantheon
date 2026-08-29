using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.ToggleRecipe;

public sealed class ToggleRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithToggleResponse()
    {
        // Arrange
        var ct = CancellationToken.None;
        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());
        var expected = new ToggleRecipeResponse(recipeId, true);

        _bus.InvokeAsync<ToggleRecipeResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await ToggleRecipeEndpoint.Handle(recipeId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<ToggleRecipeResponse>>();
        await _bus.Received(1)
            .InvokeAsync<ToggleRecipeResponse>(
                Arg.Is<ToggleRecipeInput>(input => input.RecipeId == recipeId),
                ct
            );
    }
}
