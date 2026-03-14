using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

public class Forecast : BaseEntity<Id<Forecast>>
{
    protected Forecast(Id<Forecast> id) : base(id)
    {
        Latest = new ForecastPoint(0, 0, 0, 0);
    }

    public Id<Market> MarketId { get; init; }

    public Id<Symbol> SymbolId { get; init; }

    public ForecastPoint Latest { get; init; }

    public virtual required ICollection<ForecastPoint> Predictions { get; init; }

    public virtual required Symbol Symbol { get; init; }

    public static Forecast Create(
        Id<Forecast> id,
        Market market,
        Symbol symbol,
        ForecastPoint latest,
        ICollection<ForecastPoint> predictions
    )
    {
        Guard.Against.Null(market);
        Guard.Against.Null(symbol);
        Guard.Against.Null(latest);
        Guard.Against.NullOrEmpty(predictions);

        return new Forecast(id)
        {
            SymbolId = symbol.Id,
            MarketId = market.Id,
            Latest = latest,
            Predictions = predictions,
            Symbol = symbol
        };
    }
}