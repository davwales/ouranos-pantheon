using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Interfaces.Subscriptions;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Subscriptions.SetupSubscriptions;

public sealed class SetupSubscriptionsHandler : IRequestHandler<SetupSubscriptionsInput>
{
    private readonly ILogger<SetupSubscriptionsHandler> _logger;
    private readonly ISetupSubscriptions _setupSubscriptions;

    public SetupSubscriptionsHandler(
        ILogger<SetupSubscriptionsHandler> logger,
        ISetupSubscriptions setupSubscriptions
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(setupSubscriptions);

        _logger = logger;
        _setupSubscriptions = setupSubscriptions;
    }

    public async Task Handle(
        SetupSubscriptionsInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle setup subscription request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        await _setupSubscriptions.Setup(cancellationToken);

        _logger.LogDebug("Successfully handled setup subscriptions request.");
    }
}