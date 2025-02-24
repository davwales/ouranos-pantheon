using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

public sealed class Forecast : BaseEntity<Id<Forecast>>
{
    public Forecast(
        Id<Forecast> id,
        Id<Symbol> symbolId,
        ForecastPoint latest,
        IReadOnlyList<ForecastPoint> predictions
    ) : base(id)
    {
        Guard.Against.Null(symbolId);
        Guard.Against.Null(latest);
        Guard.Against.NullOrEmpty(predictions);

        SymbolId = symbolId;
        Latest = latest;
        Predictions = predictions;
    }

    public Id<Symbol> SymbolId { get; init; }

    public ForecastPoint Latest { get; init; }

    public IReadOnlyList<ForecastPoint> Predictions { get; init; }
}