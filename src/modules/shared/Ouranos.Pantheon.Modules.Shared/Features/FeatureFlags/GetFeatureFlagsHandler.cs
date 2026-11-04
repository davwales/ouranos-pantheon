using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags.Schemas;
using Ouranos.Pantheon.Modules.Shared.Infra.Flagsmith;

namespace Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags;

public sealed class GetFeatureFlagsHandler
    : IPantheonHandler<GetFeatureFlagsRequest, GetFeatureFlagsResponse>
{
    private readonly IFlagsmithClient _flagsmith;
    private readonly ILogger<GetFeatureFlagsHandler> _logger;
    private readonly IOptions<FlagsmithOptions> _options;

    public GetFeatureFlagsHandler(
        ILogger<GetFeatureFlagsHandler> logger,
        IFlagsmithClient flagsmith,
        IOptions<FlagsmithOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(flagsmith);
        Guard.Against.Null(options);

        _logger = logger;
        _flagsmith = flagsmith;
        _options = options;
    }

    public async Task<GetFeatureFlagsResponse> Handle(
        GetFeatureFlagsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to get feature flags.");
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();

        // Report what handlers see: configured defaults stand in for flags Flagsmith did not
        // return, including all of them when it is unreachable (AllFlags is then null).
        var states = new Dictionary<string, bool>(_options.Value.DefaultFlags);
        foreach (var flag in flags.AllFlags() ?? [])
        {
            states[flag.GetFeatureName()] = flag.Enabled;
        }

        _logger.LogDebug("Successfully got {Count} feature flags.", states.Count);
        return new GetFeatureFlagsResponse(states);
    }
}
