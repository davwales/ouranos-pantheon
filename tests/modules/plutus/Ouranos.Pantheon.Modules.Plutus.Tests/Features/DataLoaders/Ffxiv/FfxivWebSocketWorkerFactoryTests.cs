using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv;

public sealed class FfxivWebSocketWorkerFactoryTests
{
    [Fact]
    public void Create_ShouldBuildAWorkerFromTheRegisteredServices()
    {
        // Arrange
        using var telemetry = new WebSocketTelemetry();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(telemetry)
            .AddSingleton(Substitute.For<IGetItems>())
            .AddSingleton(Substitute.For<IQueueTradeMessages>())
            .AddSingleton(
                Options.Create(
                    new FfxivDataLoaderOptions() with
                    {
                        WebSocket = new WebSocketOptions("wss://universalis.test/api/ws", 4096),
                    }
                )
            )
            .AddTransient<FfxivListener>()
            .AddTransient<FfxivSubscriptionInitializer>();
        using var provider = services.BuildServiceProvider();

        // Act
        using var worker = FfxivWebSocketWorkerFactory.Create(provider);

        // Assert
        worker.ShouldNotBeNull();
    }

    [Fact]
    public void Create_WhenListenerIsNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var telemetry = new WebSocketTelemetry();
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(telemetry)
            .AddSingleton(Options.Create(new FfxivDataLoaderOptions()))
            .BuildServiceProvider();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => FfxivWebSocketWorkerFactory.Create(provider));
    }
}
