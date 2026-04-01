using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.UpdateMarket;

public sealed class UpdateMarketEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new UpdateMarketInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Updated Market",
            new Taxes(null)
        );
        var expected = new IdResponse<Market>(new Id<Market>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<IdResponse<Market>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateMarketEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Market>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Market>>(input, ct);
    }
}
