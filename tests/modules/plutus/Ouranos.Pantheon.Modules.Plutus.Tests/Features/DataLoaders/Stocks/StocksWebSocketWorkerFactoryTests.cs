using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks;

public sealed class StocksWebSocketWorkerFactoryTests
{
    [Fact]
    public void Create_ShouldBuildAWorkerFromTheRegisteredServices()
    {
        // Arrange
        using var telemetry = new WebSocketTelemetry();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(telemetry)
            .AddSingleton(Substitute.For<IQueueTradeMessages>())
            .AddSingleton(
                Options.Create(
                    new StocksDataLoaderOptions() with
                    {
                        WebSocket = new WebSocketOptions("wss://alpaca.test/stream", 4096),
                    }
                )
            )
            .AddTransient<StocksTradeListener>()
            .AddTransient<StocksSuccessListener>()
            .AddTransient<StocksSubscriptionListener>()
            .AddTransient<StocksErrorListener>();
        using var provider = services.BuildServiceProvider();

        // Act
        using var worker = StocksWebSocketWorkerFactory.Create(provider);

        // Assert
        worker.ShouldNotBeNull();
    }

    [Fact]
    public void Create_WhenListenersAreNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var telemetry = new WebSocketTelemetry();
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(telemetry)
            .AddSingleton(Options.Create(new StocksDataLoaderOptions()))
            .BuildServiceProvider();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            StocksWebSocketWorkerFactory.Create(provider)
        );
    }
}
