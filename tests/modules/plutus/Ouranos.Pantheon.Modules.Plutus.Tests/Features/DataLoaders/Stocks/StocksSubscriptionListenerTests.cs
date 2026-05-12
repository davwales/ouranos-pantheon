using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks;

public sealed class StocksSubscriptionListenerTests
{
    private readonly ILogger<StocksSubscriptionListener> _logger = Substitute.For<
        ILogger<StocksSubscriptionListener>
    >();

    private readonly IWebSocketClient _client = Substitute.For<IWebSocketClient>();
    private readonly StocksSubscriptionListener _listener;

    public StocksSubscriptionListenerTests()
    {
        _listener = new StocksSubscriptionListener(_logger);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenCalled_ShouldComplete()
    {
        // Arrange
        var message = new SubscriptionAckMessage(
            Trades: ["AAPL", "MSFT"],
            Quotes: [],
            Bars: [],
            UpdatedBars: [],
            DailyBars: [],
            Statuses: [],
            Lulds: [],
            Corrections: [],
            CancelErrors: []
        );

        // Act
        var act = async () =>
            await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task HandleMessageAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var message = new SubscriptionAckMessage(
            Trades: [],
            Quotes: [],
            Bars: [],
            UpdatedBars: [],
            DailyBars: [],
            Statuses: [],
            Lulds: [],
            Corrections: [],
            CancelErrors: []
        );
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await _listener.HandleMessageAsync(message, _client, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
