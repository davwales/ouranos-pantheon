namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

public sealed record TickerQHealthData(
    int Healthy,
    int Failed,
    int Overdue,
    int NeverRan,
    int Total
);
