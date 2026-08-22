using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.CreateMarket;

public sealed class CreateMarketEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreateMarketInput("Test Market", new Taxes(null));
        var expected = new IdResponse<Market>(new Id<Market>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<IdResponse<Market>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateMarketEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<IdResponse<Market>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Market>>(input, ct);
    }
}
