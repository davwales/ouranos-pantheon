using MediatR;
using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Trades.ProcessTrade;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Messages;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer;

public sealed class Listener : IListener<SaleMessage>
{
    private readonly ILogger<Listener> _logger;
    private readonly IMediator _mediator;

    public Listener(
        ILogger<Listener> logger,
        IMediator mediator
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);

        _logger = logger;
        _mediator = mediator;
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
            )).ToList()
        );
        await _mediator.Send(processMessageRequest, cancellationToken);

        _logger.LogDebug("Successfully handled message.");
        await Task.CompletedTask;
    }
}