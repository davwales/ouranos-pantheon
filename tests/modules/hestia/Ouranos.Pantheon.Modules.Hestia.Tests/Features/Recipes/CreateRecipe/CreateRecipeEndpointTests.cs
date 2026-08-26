using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.CreateRecipe;

public sealed class CreateRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreatedWithLocationHeader()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid().ToString();
        var input = new CreateRecipeInput(
            "Chocolate Cake",
            null,
            [new StepInput("Mix and bake.")],
            [new IngredientInput(0m, "", "flour"), new IngredientInput(0m, "", "sugar")],
            "Warm."
        );
        var expected = new IdResponse<Recipe>(new Id<Recipe>(id));

        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateRecipeEndpoint.Handle(input, _bus, ct);

        // Assert
        var created = result.ShouldBeOfType<Created<IdResponse<Recipe>>>();
        created.Location.ShouldBe($"/api/hestia/recipes/{id}");
        created.Value.ShouldBe(expected);
        await _bus.Received(1).InvokeAsync<IdResponse<Recipe>>(input, ct);
    }
}
