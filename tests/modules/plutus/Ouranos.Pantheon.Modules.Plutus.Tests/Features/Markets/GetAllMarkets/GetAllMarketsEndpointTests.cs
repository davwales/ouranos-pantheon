using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.GetAllMarkets;

public sealed class GetAllMarketsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllMarketsInput();
        var expected = new List<GetAllMarketsResponse>();

        _bus.InvokeAsync<List<GetAllMarketsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllMarketsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAllMarketsResponse>>>();
        await _bus.Received(1).InvokeAsync<List<GetAllMarketsResponse>>(input, ct);
    }
}
