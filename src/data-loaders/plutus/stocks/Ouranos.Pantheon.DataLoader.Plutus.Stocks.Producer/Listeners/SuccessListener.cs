using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Messages;

namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Listeners;

public sealed class SuccessListener : IListener<SuccessMessage>
{
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly ILogger<SuccessListener> _logger;
    private readonly IReadOnlyCollection<string> _symbols;

    public SuccessListener(
        ILogger<SuccessListener> logger,
        IOptions<AlpacaOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(options);
        Guard.Against.Null(options.Value);

        _logger = logger;
        _symbols = options.Value.Symbols;
        _apiKey = options.Value.ApiKey;
        _apiSecret = options.Value.ApiSecret;
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
            var authMessage = new AuthMessage(_apiKey, _apiSecret);
            await client.SendAsync(authMessage, cancellationToken);
        }

        const string authenticatedFlag = "authenticated";
        if (message.Msg == authenticatedFlag)
        {
            var subscribeMessage = new SubscribeMessage([.._symbols], [], []);
            await client.SendAsync(subscribeMessage, cancellationToken);
        }

        _logger.LogInformation("Successfully handled success message.");
    }
}