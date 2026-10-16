using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetForecastLeaders.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetForecastLeaders;

public sealed class GetForecastLeadersTool(
    ILogger<GetForecastLeadersTool> logger,
    IMessageBus bus,
    IOptions<PlutusOptions> options
) : AssistantDataTool<MarketAnalystContext, TakeArguments, ForecastLeadersData>(logger)
{
    internal const string ForecastingDisabled = "Forecasting is disabled for this market.";

    internal const string NoForecasts = "No forecasts are available for this market.";

    private const string DayOneGain =
        $"{nameof(GetMarketForecastResponse.DayOne)}.{nameof(GetMarketForecastPredictionResponse.Gain)}";

    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IOptions<PlutusOptions> _options = Guard.Against.Null(options);

    public override string Name => "get_forecast_leaders";

    public override string Description =>
        "Symbols with the highest forecast gain after tax for tomorrow, with their ids"
        + "and their current, day 1 and day 7 forecast prices; gain is margin * volume.";

    protected override async ValueTask<AssistantData<ForecastLeadersData>> QueryAsync(
        MarketAnalystContext context,
        TakeArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var market = await _bus.InvokeAsync<GetMarketResponse>(
            new GetMarketInput(context.MarketId),
            cancellationToken
        );

        if (!market.IsForecastingEnabled)
        {
            return AssistantData<ForecastLeadersData>.WithNote(ForecastingDisabled);
        }

        var forecasts = await _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
            new GetMarketForecastInput(
                context.MarketId,
                DayOneGain,
                "desc",
                Take: _options.Value.MarketAnalyst.ClampRows(arguments.Take)
            ),
            cancellationToken
        );

        if (!forecasts.Items.Any())
        {
            return AssistantData<ForecastLeadersData>.WithNote(NoForecasts);
        }

        return new ForecastLeadersData([
            .. forecasts.Items.Select(forecast => new ForecastLeaderData(
                forecast.SymbolId,
                forecast.SymbolName,
                MarketAnalystData.ForecastOutlook(forecast)
            )),
        ]);
    }
}
