using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks;

public sealed class StocksTradeListenerTests
{
    private readonly ILogger<StocksTradeListener> _logger = Substitute.For<
        ILogger<StocksTradeListener>
    >();
    private readonly IQueueTradeMessages _queue = Substitute.For<IQueueTradeMessages>();
    private readonly IWebSocketClient _client = Substitute.For<IWebSocketClient>();
    private readonly StocksTradeListener _listener;

    public StocksTradeListenerTests()
    {
        _listener = new StocksTradeListener(_logger, _queue);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenCalled_ShouldQueueTradeMessage()
    {
        // Arrange
        var message = new AlpacaTradeMessage(
            SymbolCode: "AAPL",
            TradeId: 42,
            ExchangeCode: "NYSE",
            Price: 150.25m,
            Size: 100,
            Conditions: ["@", " "],
            Timestamp: DateTimeOffset.UtcNow,
            Tape: "C",
            Vwap: null
        );

        // Act
        await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await _queue
            .Received(1)
            .QueueMessages(
                Arg.Is<IReadOnlyCollection<TradeMessage>>(msgs =>
                    msgs.Count == 1
                    && msgs.First().SymbolCode == "AAPL"
                    && msgs.First().Price == 150.25m
                    && msgs.First().Producer == Producer.Stocks
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task HandleMessageAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var message = new AlpacaTradeMessage(
            "AAPL",
            1,
            "NYSE",
            100m,
            10,
            [],
            DateTimeOffset.UtcNow,
            "C",
            null
        );
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await _listener.HandleMessageAsync(message, _client, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
