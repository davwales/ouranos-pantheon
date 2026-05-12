using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetAllTrades;

public sealed class GetAllTradesEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllTradesInput(Take: 10);
        var expected = new List<GetAllTradesResponse>();

        _bus.InvokeAsync<List<GetAllTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllTradesEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAllTradesResponse>>>();
        await _bus.Received(1).InvokeAsync<List<GetAllTradesResponse>>(input, ct);
    }
}
