using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Application.Commands.Trades.ProcessTrade;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Messages;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer;

public sealed class Listener : IListener<SaleMessage>
{
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<Listener> _logger;

    public Listener(
        ILogger<Listener> logger,
        IDispatcher dispatcher
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dispatcher);

        _logger = logger;
        _dispatcher = dispatcher;
    }

    public async Task HandleMessageAsync(
        SaleMessage message,
        IWebSocketClient _,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Handling message '{message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        var processMessageRequest = new ProcessTradeInput(
            message.Item.ToString(),
            message.Sales.Select(s => new ProcessTradeSaleInput(
                    s.Hq,
                    s.PricePerUnit,
                    s.Quantity,
                    DateTimeOffset.FromUnixTimeSeconds(s.Timestamp)
                )
            ).ToList()
        );
        await _dispatcher.Send(processMessageRequest, cancellationToken);

        _logger.LogDebug("Successfully handled message.");
    }
}