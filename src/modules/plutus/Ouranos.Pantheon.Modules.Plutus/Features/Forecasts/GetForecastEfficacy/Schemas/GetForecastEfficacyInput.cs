namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;

public sealed record GetForecastEfficacyInput(
    string? SymbolId = null,
    string? MarketId = null,
    string? ModelName = null,
    int? HorizonDays = null,
    DateTimeOffset? Since = null,
    DateTimeOffset? Until = null,
    int Skip = 0,
    int Take = 10
);
