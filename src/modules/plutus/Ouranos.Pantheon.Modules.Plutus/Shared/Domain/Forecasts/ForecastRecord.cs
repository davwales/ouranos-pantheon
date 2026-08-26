using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

public sealed class ForecastRecord : BaseEntity<Id<ForecastRecord>>
{
    private ForecastRecord(Id<ForecastRecord> id)
        : base(id)
    {
        Predicted = new ForecastPoint(0, 0, 0, 0);
    }

    public Id<ForecastRun> RunId { get; init; }

    public Id<Market> MarketId { get; init; }

    public Id<Symbol> SymbolId { get; init; }

    public string ModelName { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; init; }

    public DateTimeOffset TargetAt { get; init; }

    public int HorizonDays { get; init; }

    public ForecastPoint Predicted { get; init; }

    public ForecastRun? Run { get; private set; }

    public Symbol? Symbol { get; private set; }

    public static ForecastRecord Create(
        Id<ForecastRecord> id,
        Id<ForecastRun> runId,
        Id<Market> marketId,
        Id<Symbol> symbolId,
        string modelName,
        DateTimeOffset generatedAt,
        DateTimeOffset targetAt,
        int horizonDays,
        ForecastPoint predicted
    )
    {
        Guard.Against.NullOrWhiteSpace(modelName);
        Guard.Against.Null(predicted);
        Guard.Against.NegativeOrZero(horizonDays);

        return new ForecastRecord(id)
        {
            RunId = runId,
            MarketId = marketId,
            SymbolId = symbolId,
            ModelName = modelName,
            GeneratedAt = generatedAt,
            TargetAt = targetAt,
            HorizonDays = horizonDays,
            Predicted = predicted,
        };
    }
}
