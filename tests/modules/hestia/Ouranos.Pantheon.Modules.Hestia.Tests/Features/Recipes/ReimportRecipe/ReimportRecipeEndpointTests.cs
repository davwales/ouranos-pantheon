using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ReimportRecipe;

public sealed class ReimportRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnAcceptedWithRecipeId()
    {
        // Arrange
        var ct = CancellationToken.None;
        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());
        var expected = new IdResponse<Recipe>(recipeId);

        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await ReimportRecipeEndpoint.Handle(recipeId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Accepted<IdResponse<Recipe>>>();
        await _bus.Received(1)
            .InvokeAsync<IdResponse<Recipe>>(
                Arg.Is<ReimportRecipeInput>(input => input.RecipeId == recipeId),
                ct
            );
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var ct = CancellationToken.None;
        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());
        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException<IdResponse<Recipe>>(
                    new NotFoundException("Recipe", recipeId.ToString())
                )
            );

        // Act
        var handle = async () => await ReimportRecipeEndpoint.Handle(recipeId, _bus, ct);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }
}
