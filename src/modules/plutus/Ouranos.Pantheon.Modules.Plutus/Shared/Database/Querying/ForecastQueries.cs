using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.Querying;

public static class ForecastQueries
{
    public static IQueryable<Forecast> WhereLatestPerSymbol(this IQueryable<Forecast> forecasts)
    {
        return forecasts.Where(f =>
            f.CreatedAt
            == forecasts
                .Where(f2 => f2.MarketId == f.MarketId && f2.SymbolId == f.SymbolId)
                .Max(f2 => f2.CreatedAt)
        );
    }
}
