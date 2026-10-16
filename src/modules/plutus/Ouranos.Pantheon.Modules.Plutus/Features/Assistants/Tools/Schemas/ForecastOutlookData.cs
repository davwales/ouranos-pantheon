namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

public sealed record ForecastOutlookData(
    decimal CurrentAveragePrice,
    decimal DayOneAveragePrice,
    decimal DayOneGainAfterTax,
    decimal DaySevenAveragePrice,
    decimal DaySevenGainAfterTax
);
