using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks;

public sealed class StocksSuccessListenerTests
{
    private readonly ILogger<StocksSuccessListener> _logger = Substitute.For<
        ILogger<StocksSuccessListener>
    >();
    private readonly IWebSocketClient _client = Substitute.For<IWebSocketClient>();
    private readonly StocksDataLoaderOptions _options;
    private readonly StocksSuccessListener _listener;

    public StocksSuccessListenerTests()
    {
        _options = new StocksDataLoaderOptions(
            IsEnabled: true,
            ApiKey: "test-key",
            ApiSecret: "test-secret",
            WebSocket: new WebSocketOptions(),
            Symbols: ["AAPL", "MSFT"]
        );
        _listener = new StocksSuccessListener(_logger, Options.Create(_options));
    }

    [Fact]
    public async Task HandleMessageAsync_WhenConnected_ShouldSendAuthMessage()
    {
        // Arrange
        var message = new SuccessMessage("connected");

        // Act
        await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await _client
            .Received(1)
            .SendAsync(
                Arg.Is<AuthMessage>(m => m.Key == "test-key" && m.Secret == "test-secret"),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task HandleMessageAsync_WhenAuthenticated_ShouldSendSubscribeMessage()
    {
        // Arrange
        var message = new SuccessMessage("authenticated");

        // Act
        await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await _client
            .Received(1)
            .SendAsync(
                Arg.Is<SubscribeMessage>(m =>
                    m.Trades.Contains("AAPL") && m.Trades.Contains("MSFT")
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task HandleMessageAsync_WhenOtherMessage_ShouldNotSendAnyMessage()
    {
        // Arrange
        var message = new SuccessMessage("something-else");

        // Act
        await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await _client.DidNotReceive().SendAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleMessageAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var message = new SuccessMessage("connected");
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await _listener.HandleMessageAsync(message, _client, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
