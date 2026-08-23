using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnAcceptedWithLocationHeader()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid().ToString();
        var input = new ImportRecipeInput("https://example.com/recipe");
        var expected = new IdResponse<Recipe>(new Id<Recipe>(id));

        _bus.InvokeAsync<IdResponse<Recipe>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await ImportRecipeEndpoint.Handle(input, _bus, ct);

        // Assert
        var accepted = result.ShouldBeOfType<Accepted<IdResponse<Recipe>>>();
        accepted.Location.ShouldBe($"/api/hestia/recipes/{id}");
        accepted.Value.ShouldBe(expected);
        await _bus.Received(1).InvokeAsync<IdResponse<Recipe>>(input, ct);
    }
}
