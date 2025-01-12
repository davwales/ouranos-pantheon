using MediatR;
using Ouranos.Pantheon.Core.WebSockets.Interfaces;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Messages.ProcessMessage;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Subscriptions.SetupSubscriptions;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker;

public sealed class Listener : IListener
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

    public async Task OnConnectedAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Attempt to initialize listener.");
        cancellationToken.ThrowIfCancellationRequested();

        var setupSubscriptionsRequest = new SetupSubscriptionsInput();
        await _mediator.Send(setupSubscriptionsRequest, cancellationToken);

        _logger.LogDebug("Successfully initialized listener.");
    }

    public async Task HandleMessageAsync(
        byte[] message,
        CancellationToken cancellationToken
    )
    {
        _logger.LogTrace("Handling message '{message}'.", message);
        cancellationToken.ThrowIfCancellationRequested();

        var processMessageRequest = new ProcessMessageInput(message);
        await _mediator.Send(processMessageRequest, cancellationToken);

        _logger.LogDebug("Successfully handled message.");
        await Task.CompletedTask;
    }
}