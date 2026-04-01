using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.UpdateRecipe;

public sealed class UpdateRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new UpdateRecipeInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            new Id<Recipe>(Guid.NewGuid().ToString()),
            "Updated Recipe",
            10.0m,
            [],
            []
        );
        var expected = new IdResponse<Recipe>(new Id<Recipe>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateRecipeEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Recipe>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Recipe>>(input, ct);
    }
}
