using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags;
using Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.FeatureFlags;

public sealed class FeatureFlagsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var expected = new GetFeatureFlagsResponse(new() { ["module.flag"] = true });

        _bus.InvokeAsync<GetFeatureFlagsResponse>(Arg.Any<GetFeatureFlagsRequest>(), ct)
            .Returns(Task.FromResult(expected));

        // Act
        var result = await FeatureFlagsEndpoint.Handle(_bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetFeatureFlagsResponse>>().Value.ShouldBeSameAs(expected);
        await _bus.Received(1)
            .InvokeAsync<GetFeatureFlagsResponse>(new GetFeatureFlagsRequest(), ct);
    }
}
