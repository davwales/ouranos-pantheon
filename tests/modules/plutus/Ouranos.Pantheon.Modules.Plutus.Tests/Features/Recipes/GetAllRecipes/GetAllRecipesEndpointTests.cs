using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.GetAllRecipes;

public sealed class GetAllRecipesEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllRecipesInput();
        var expected = new PagedResponse<GetAllRecipesResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetAllRecipesResponse>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllRecipesEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetAllRecipesResponse>>>();
        await _bus.Received(1).InvokeAsync<PagedResponse<GetAllRecipesResponse>>(input, ct);
    }
}
