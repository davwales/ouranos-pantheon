using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetVolumeHeatmap;

public sealed class GetVolumeHeatmapEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalledWithDefaultLookbackWeeks_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var expected = new GetVolumeHeatmapResponse([]);

        _bus.InvokeAsync<GetVolumeHeatmapResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetVolumeHeatmapEndpoint.Handle(marketId, _bus, ct: ct);

        // Assert
        result.ShouldBeOfType<Ok<GetVolumeHeatmapResponse>>();
        await _bus.Received(1)
            .InvokeAsync<GetVolumeHeatmapResponse>(
                Arg.Is<GetVolumeHeatmapInput>(i => i.LookbackWeeks == 4),
                ct
            );
    }

    [Fact]
    public async Task Handle_WhenCalledWithExplicitLookbackWeeks_ShouldPassToInput()
    {
        // Arrange
        var ct = CancellationToken.None;
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var expected = new GetVolumeHeatmapResponse([]);

        _bus.InvokeAsync<GetVolumeHeatmapResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetVolumeHeatmapEndpoint.Handle(marketId, _bus, 2, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetVolumeHeatmapResponse>>();
        await _bus.Received(1)
            .InvokeAsync<GetVolumeHeatmapResponse>(
                Arg.Is<GetVolumeHeatmapInput>(i => i.LookbackWeeks == 2),
                ct
            );
    }
}
