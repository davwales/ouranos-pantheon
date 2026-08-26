using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

public sealed class ForecastRun : BaseEntity<Id<ForecastRun>>
{
    private ForecastRun(Id<ForecastRun> id)
        : base(id) { }

    public string ModelName { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; init; }

    public static ForecastRun Create(
        Id<ForecastRun> id,
        string modelName,
        DateTimeOffset generatedAt
    )
    {
        Guard.Against.NullOrWhiteSpace(modelName);

        return new ForecastRun(id) { ModelName = modelName, GeneratedAt = generatedAt };
    }
}
