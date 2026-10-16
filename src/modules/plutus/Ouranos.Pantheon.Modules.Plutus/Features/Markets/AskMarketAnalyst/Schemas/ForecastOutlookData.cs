namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;

public sealed record ForecastOutlookData(
    decimal CurrentAveragePrice,
    decimal DayOneAveragePrice,
    decimal DayOneGainAfterTax,
    decimal DaySevenAveragePrice,
    decimal DaySevenGainAfterTax
);
