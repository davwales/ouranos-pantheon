using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.PublicMode.GetPublicMode.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.PublicMode.GetPublicMode;

public sealed class GetPublicModeHandler(
    ILogger<GetPublicModeHandler> logger,
    IFlagsmithClient flagsmith
) : IPantheonHandler<GetPublicModeInput, GetPublicModeResponse>
{
    private readonly IFlagsmithClient _flagsmith = Guard.Against.Null(flagsmith);
    private readonly ILogger<GetPublicModeHandler> _logger = Guard.Against.Null(logger);

    public async Task<GetPublicModeResponse> Handle(
        GetPublicModeInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get public mode query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        var isEnabled = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        _logger.LogDebug("Successfully handled get public mode request.");
        return new GetPublicModeResponse(isEnabled);
    }
}
