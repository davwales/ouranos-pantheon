namespace Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags.Schemas;

public sealed record GetFeatureFlagsResponse(Dictionary<string, bool> Flags);
