using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.GetRecipeHistory;

public sealed class GetRecipeHistoryEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithRecipeHistory()
    {
        // Arrange
        var ct = CancellationToken.None;
        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());
        var expected = new GetRecipeHistoryResponse(
            recipeId,
            [new RecipeHistoryEventResponse(1, "recipe_created", DateTimeOffset.UtcNow)]
        );

        _bus.InvokeAsync<GetRecipeHistoryResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetRecipeHistoryEndpoint.Handle(recipeId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetRecipeHistoryResponse>>();
        await _bus.Received(1)
            .InvokeAsync<GetRecipeHistoryResponse>(
                Arg.Is<GetRecipeHistoryInput>(input => input.RecipeId == recipeId),
                ct
            );
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var ct = CancellationToken.None;
        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());
        _bus.InvokeAsync<GetRecipeHistoryResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException<GetRecipeHistoryResponse>(
                    new NotFoundException("Recipe", recipeId.ToString())
                )
            );

        // Act
        var handle = async () => await GetRecipeHistoryEndpoint.Handle(recipeId, _bus, ct);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }
}
