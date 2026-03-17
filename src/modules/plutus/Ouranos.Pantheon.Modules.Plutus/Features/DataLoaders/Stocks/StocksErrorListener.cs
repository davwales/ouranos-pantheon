using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;

public sealed class StocksErrorListener : IListener<ErrorMessage>
{
    private readonly ILogger<StocksErrorListener> _logger;

    public StocksErrorListener(ILogger<StocksErrorListener> logger)
    {
        Guard.Against.Null(logger);
        _logger = logger;
    }

    public async Task HandleMessageAsync(
        ErrorMessage message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogError("Received error message with code '{code}', msg '{msg}'.", message.Code, message.Msg);
        await Task.CompletedTask;
    }
}
