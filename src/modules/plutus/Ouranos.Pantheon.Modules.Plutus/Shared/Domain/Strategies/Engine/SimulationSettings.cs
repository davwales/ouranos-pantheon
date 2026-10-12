namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;

/// <param name="Start">First decision point.</param>
/// <param name="End">Inventory still held here is liquidated.</param>
/// <param name="Budget">Starting cash.</param>
/// <param name="ParticipationRate">Share of traded volume the strategy can capture.</param>
/// <param name="SlippageMultiplier">Square-root market impact coefficient.</param>
/// <param name="Progress">Receives the completed fraction, 0 to 1.</param>
/// <param name="CancellationToken">Checked once per simulated day.</param>
public sealed record SimulationSettings(
    DateTimeOffset Start,
    DateTimeOffset End,
    decimal Budget,
    decimal ParticipationRate,
    decimal SlippageMultiplier,
    IProgress<double>? Progress = null,
    CancellationToken CancellationToken = default
)
{
    public int TotalDays => (int)(End - Start).TotalDays;
}
