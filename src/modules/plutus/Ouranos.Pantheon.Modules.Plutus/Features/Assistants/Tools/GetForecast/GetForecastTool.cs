using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecast;

public sealed class GetForecastTool(ILogger<GetForecastTool> logger, IMessageBus bus)
    : AssistantDataTool<ISymbolScope, AssistantToolNoArguments, ForecastData>(logger)
{
    internal const string NoForecast = "No forecast is available for this symbol.";

    internal const int MaxEfficacyRows = 50;

    internal static readonly TimeSpan EfficacyWindow = TimeSpan.FromDays(30);

    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public override string Name => "get_forecast";

    public override string Description =>
        "Call this before discussing where the price is heading. Returns the 7-day forecast for "
        + "this symbol day by day from its baseline day, with the most-evaluated model's recent "
        + "error per horizon; weigh the forecast by that error.";

    protected override async ValueTask<AssistantData<ForecastData>> QueryAsync(
        ISymbolScope context,
        AssistantToolNoArguments arguments,
        CancellationToken cancellationToken
    )
    {
        if (
            !await AssistantToolQueries.IsForecastingEnabledAsync(
                _bus,
                context.MarketId,
                cancellationToken
            )
        )
        {
            return AssistantData<ForecastData>.WithNote(AssistantToolQueries.ForecastingDisabled);
        }

        var forecast = AssistantToolQueries.LoadForecastAsync(
            _bus,
            context.MarketId,
            context.SymbolId,
            cancellationToken
        );

        var efficacy = _bus.InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(
            new GetForecastEfficacyInput(
                SymbolId: context.SymbolId.Value,
                Since: DateTimeOffset.UtcNow - EfficacyWindow,
                Take: MaxEfficacyRows
            ),
            cancellationToken
        );

        if (await forecast is not { } loaded)
        {
            return AssistantData<ForecastData>.WithNote(NoForecast);
        }

        GetMarketForecastPredictionResponse[] days =
        [
            loaded.DayOne,
            loaded.DayTwo,
            loaded.DayThree,
            loaded.DayFour,
            loaded.DayFive,
            loaded.DaySix,
            loaded.DaySeven,
        ];

        var baseline = loaded.Latest;

        return new ForecastData(
            new ForecastPriceData(
                baseline.AveragePrice,
                baseline.MinPrice,
                baseline.MaxPrice,
                baseline.Volume
            ),
            [
                .. days.Select(
                    (day, index) =>
                        new ForecastDayData(
                            index + 1,
                            day.AveragePrice,
                            day.MinPrice,
                            day.MaxPrice,
                            day.Margin
                        )
                ),
            ],
            Error((await efficacy).Items)
        );
    }

    internal static ForecastErrorData? Error(IEnumerable<GetForecastEfficacyResponse> efficacy)
    {
        var model = efficacy
            .GroupBy(row => row.ModelName)
            .OrderByDescending(group => group.Sum(row => row.EvaluatedCount))
            .FirstOrDefault();

        var horizons = model
            ?.Where(row => row.MeanAbsolutePercentageError is not null)
            .GroupBy(row => row.HorizonDays)
            .Select(group => group.MaxBy(row => row.EvaluatedCount))
            .OfType<GetForecastEfficacyResponse>()
            .OrderBy(row => row.HorizonDays)
            .ToList();

        if (model is null || horizons is not { Count: > 0 })
        {
            return null;
        }

        return new ForecastErrorData(
            model.Key,
            EfficacyWindow.Days,
            horizons[0].EvaluatedCount,
            [
                .. horizons.Select(row => new ForecastHorizonErrorData(
                    row.HorizonDays,
                    AssistantToolData.Percent(row.MeanAbsolutePercentageError ?? 0)
                )),
            ],
            horizons[0].MeanBias
        );
    }
}
