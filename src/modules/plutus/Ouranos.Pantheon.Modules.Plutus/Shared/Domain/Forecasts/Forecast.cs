using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

public class Forecast : BaseEntity<Id<Forecast>>
{
    protected Forecast(Id<Forecast> id)
        : base(id)
    {
        Latest = new ForecastPoint(0, 0, 0, 0);
    }

    public Id<Market> MarketId { get; init; }

    public Id<Symbol> SymbolId { get; init; }

    public ForecastPoint Latest { get; init; }

    private ICollection<ForecastPoint>? _predictions;

    public ICollection<ForecastPoint> Predictions =>
        _predictions ?? throw new NavigationPropertyNotLoadedException<Forecast>();

    private Symbol? _symbol;
    public Symbol Symbol => _symbol ?? throw new NavigationPropertyNotLoadedException<Forecast>();

    public static Forecast Create(
        Id<Forecast> id,
        Id<Market> marketId,
        Id<Symbol> symbolId,
        ForecastPoint latest,
        ICollection<ForecastPoint> predictions,
        Symbol? symbol = null
    )
    {
        Guard.Against.Null(latest);
        Guard.Against.NullOrEmpty(predictions);

        if (symbol is not null)
        {
            Guard.Against.InvalidInput(symbol, nameof(symbol), s => s.Id == symbolId);
        }

        return new Forecast(id)
        {
            MarketId = marketId,
            SymbolId = symbolId,
            Latest = latest,
            _predictions = predictions,
            _symbol = symbol,
        };
    }
}
