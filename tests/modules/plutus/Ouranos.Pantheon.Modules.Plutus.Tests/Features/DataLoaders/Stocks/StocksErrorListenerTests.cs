using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks;

public sealed class StocksErrorListenerTests
{
    private readonly ILogger<StocksErrorListener> _logger = Substitute.For<ILogger<StocksErrorListener>>();
    private readonly IWebSocketClient _client = Substitute.For<IWebSocketClient>();
    private readonly StocksErrorListener _listener;

    public StocksErrorListenerTests()
    {
        _listener = new StocksErrorListener(_logger);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenCalled_ShouldComplete()
    {
        // Arrange
        var message = new ErrorMessage(401, "Unauthorized");

        // Act
        var act = async () => await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
    }
}
