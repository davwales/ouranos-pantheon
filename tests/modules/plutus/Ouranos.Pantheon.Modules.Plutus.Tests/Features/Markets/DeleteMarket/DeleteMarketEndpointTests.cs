using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.DeleteMarket;

public sealed class DeleteMarketEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var expected = new IdResponse<Market>(marketId);

        _bus.InvokeAsync<IdResponse<Market>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteMarketEndpoint.Handle(marketId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Market>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Market>>(Arg.Any<DeleteMarketInput>(), ct);
    }
}
