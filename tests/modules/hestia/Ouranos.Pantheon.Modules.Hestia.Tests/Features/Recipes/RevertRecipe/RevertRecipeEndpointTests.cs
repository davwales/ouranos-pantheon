using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.RevertRecipe;

public sealed class RevertRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithRecipeId()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid().ToString();
        var recipeId = new Id<Recipe>(id);
        var targetVersion = 3L;
        var body = new RevertRecipeBody(targetVersion);
        var expected = new IdResponse<Recipe>(recipeId);

        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await RevertRecipeEndpoint.Handle(recipeId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Recipe>>>();
        await _bus.Received(1)
            .InvokeAsync<IdResponse<Recipe>>(
                Arg.Is<RevertRecipeInput>(input =>
                    input.RecipeId == recipeId && input.TargetVersion == targetVersion
                ),
                ct
            );
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var ct = CancellationToken.None;
        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());
        var body = new RevertRecipeBody(1);
        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException<IdResponse<Recipe>>(
                    new NotFoundException("Recipe", recipeId.ToString())
                )
            );

        // Act
        var handle = async () => await RevertRecipeEndpoint.Handle(recipeId, body, _bus, ct);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }
}
