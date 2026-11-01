using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecastLeaders.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecastLeaders;

public sealed class GetForecastLeadersTool(
    ILogger<GetForecastLeadersTool> logger,
    IMessageBus bus,
    IOptions<PlutusOptions> options
) : AssistantDataTool<IMarketScope, TakeArguments, ForecastLeadersData>(logger)
{
    internal const string NoForecasts = "No forecasts are available for this market.";

    private const string DayOneGain =
        $"{nameof(GetMarketForecastResponse.DayOne)}.{nameof(GetMarketForecastPredictionResponse.Gain)}";

    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IOptions<PlutusOptions> _options = Guard.Against.Null(options);

    public override string Name => "get_forecast_leaders";

    public override string Description =>
        "Symbols with the highest forecast gain after tax for tomorrow, with their ids "
        + "and their current, day 1 and day 7 forecast prices; gain is margin * volume.";

    protected override async ValueTask<AssistantData<ForecastLeadersData>> QueryAsync(
        IMarketScope context,
        TakeArguments arguments,
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
            return AssistantData<ForecastLeadersData>.WithNote(
                AssistantToolQueries.ForecastingDisabled
            );
        }

        var forecasts = await _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
            new GetMarketForecastInput(
                context.MarketId,
                DayOneGain,
                SortDirection.Desc.ToQueryValue(),
                Take: _options.Value.AssistantTools.ClampRows(arguments.Take)
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
                AssistantToolData.ForecastOutlook(forecast)
            )),
        ]);
    }
}
