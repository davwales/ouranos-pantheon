using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.UpdateStrategy;

public sealed class UpdateStrategyEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly IOutputCacheStore _outputCacheStore = Substitute.For<IOutputCacheStore>();
    private readonly Id<Strategy> _strategyId = new(Guid.NewGuid().ToString());

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldEvictStrategyRecommendations()
    {
        // Arrange
        var ct = CancellationToken.None;

        _bus.InvokeAsync<IdResponse<Strategy>>(Arg.Any<UpdateStrategyInput>(), ct)
            .Returns(new IdResponse<Strategy>(_strategyId));

        // Act
        var result = await UpdateStrategyEndpoint.Handle(
            _strategyId,
            new UpdateStrategyBody("Name", null, StrategyTestFactory.Swing()),
            _bus,
            _outputCacheStore,
            ct
        );

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Strategy>>>();

        await _outputCacheStore
            .Received(1)
            .EvictByTagAsync(RecommendationCache.TagFor(_strategyId), ct);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrows_ShouldNotEvict()
    {
        // Arrange
        var ct = CancellationToken.None;

        _bus.InvokeAsync<IdResponse<Strategy>>(Arg.Any<UpdateStrategyInput>(), ct)
            .Returns<IdResponse<Strategy>>(_ => throw new InvalidOperationException());

        // Act
        var handle = async () =>
            await UpdateStrategyEndpoint.Handle(
                _strategyId,
                new UpdateStrategyBody("Name", null, StrategyTestFactory.Swing()),
                _bus,
                _outputCacheStore,
                ct
            );

        // Assert
        await handle.ShouldThrowAsync<InvalidOperationException>();

        await _outputCacheStore
            .DidNotReceive()
            .EvictByTagAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
