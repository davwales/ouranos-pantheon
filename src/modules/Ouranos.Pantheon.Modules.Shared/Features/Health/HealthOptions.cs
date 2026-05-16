namespace Ouranos.Pantheon.Modules.Shared.Features.Health;

public sealed record HealthOptions
{
    public const string SectionName = "Ouranos:Health";

    public int PerCheckTimeoutSeconds { get; init; } = 5;
}
