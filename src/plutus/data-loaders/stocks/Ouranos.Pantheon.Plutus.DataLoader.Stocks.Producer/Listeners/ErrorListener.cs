using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Messages;

namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Listeners;

public sealed class ErrorListener : IListener<ErrorMessage>
{
    private readonly ILogger<ErrorListener> _logger;

    public ErrorListener(ILogger<ErrorListener> logger)
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