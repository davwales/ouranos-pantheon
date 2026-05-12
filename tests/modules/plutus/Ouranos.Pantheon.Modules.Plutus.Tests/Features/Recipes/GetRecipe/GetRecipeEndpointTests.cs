using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.GetRecipe;

public sealed class GetRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());
        var marketId = new Id<Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets.Market>(
            Guid.NewGuid().ToString()
        );
        var expected = new GetRecipeResponse(recipeId, marketId, "Test Recipe", 10.0m, [], []);

        _bus.InvokeAsync<GetRecipeResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetRecipeEndpoint.Handle(recipeId, TimeFrame.OneHour, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetRecipeResponse>>();
        await _bus.Received(1).InvokeAsync<GetRecipeResponse>(Arg.Any<GetRecipeInput>(), ct);
    }
}
