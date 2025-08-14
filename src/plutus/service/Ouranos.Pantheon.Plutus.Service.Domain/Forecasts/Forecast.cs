using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

public sealed class Forecast : BaseEntity<Id<Forecast>>
{
    private Forecast()
    {
    }

    public Forecast(
        Id<Forecast> id,
        Id<Market> marketId,
        Id<Symbol> symbolId,
        string symbolName,
        string? symbolSubcode,
        ForecastPoint latest,
        IReadOnlyList<ForecastPoint> predictions
    ) : base(id)
    {
        Guard.Against.Null(marketId);
        Guard.Against.Null(symbolId);
        Guard.Against.NullOrWhiteSpace(symbolName);
        Guard.Against.Null(latest);
        Guard.Against.NullOrEmpty(predictions);

        MarketId = marketId;
        SymbolId = symbolId;
        SymbolName = symbolName;
        SymbolSubcode = symbolSubcode;
        Latest = latest;
        Predictions = predictions;
    }

    public Id<Market> MarketId { get; init; }

    public Id<Symbol> SymbolId { get; init; }

    public string SymbolName { get; init; }

    public string? SymbolSubcode { get; init; }

    public ForecastPoint Latest { get; init; }

    public IReadOnlyList<ForecastPoint> Predictions { get; init; }
}