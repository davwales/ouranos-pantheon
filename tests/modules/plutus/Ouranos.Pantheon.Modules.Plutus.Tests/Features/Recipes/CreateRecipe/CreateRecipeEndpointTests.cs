using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.CreateRecipe;

public sealed class CreateRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreateRecipeInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test Recipe",
            10.0m,
            [],
            []
        );
        var expected = new IdResponse<Recipe>(new Id<Recipe>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateRecipeEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<IdResponse<Recipe>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Recipe>>(input, ct);
    }
}
