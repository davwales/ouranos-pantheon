namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

public sealed record OptimizationOptions(
    double ElitismRate,
    double MutationRate,
    double SharpeRatioWeight,
    double TotalReturnWeight,
    double MaxDrawdownWeight
)
{
    public const string SectionName = "Optimization";

    public OptimizationOptions() : this(
        ElitismRate: 0.3,
        MutationRate: 0.1,
        SharpeRatioWeight: 0.5,
        TotalReturnWeight: 0.3,
        MaxDrawdownWeight: -0.2
    )
    {
    }
}