using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Messages;

namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Listeners;

public sealed class SuccessListener : IListener<SuccessMessage>
{
    private readonly ILogger<SuccessListener> _logger;
    private readonly IOptions<AlpacaOptions> _options;

    public SuccessListener(
        ILogger<SuccessListener> logger,
        IOptions<AlpacaOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(options);

        _logger = logger;
        _options = options;
    }

    public async Task HandleMessageAsync(
        SuccessMessage message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle success message.");
        cancellationToken.ThrowIfCancellationRequested();

        const string connectedFlag = "connected";
        if (message.Msg == connectedFlag)
        {
            var authMessage = new AuthMessage(_options.Value.ApiKey, _options.Value.ApiSecret);
            await client.SendAsync(authMessage, cancellationToken);
        }

        const string authenticatedFlag = "authenticated";
        if (message.Msg == authenticatedFlag)
        {
            var subscribeMessage = new SubscribeMessage(
                [.. _options.Value.Symbols],
                [],
                []
            );
            await client.SendAsync(subscribeMessage, cancellationToken);
        }

        _logger.LogInformation("Successfully handled success message.");
    }
}