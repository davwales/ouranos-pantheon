using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetRecipeTrades;

public sealed class GetRecipeTradesEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetRecipeTradesInput(new Id<Market>(Guid.NewGuid().ToString()), Take: 10);
        var expected = new PagedResponse<GetRecipeTradesResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetRecipeTradesResponse>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetRecipeTradesEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetRecipeTradesResponse>>>();
        await _bus.Received(1).InvokeAsync<PagedResponse<GetRecipeTradesResponse>>(input, ct);
    }
}
