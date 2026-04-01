using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.DeleteRecipe;

public sealed class DeleteRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());
        var expected = new IdResponse<Recipe>(recipeId);

        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteRecipeEndpoint.Handle(recipeId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Recipe>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Recipe>>(Arg.Any<DeleteRecipeInput>(), ct);
    }
}
