using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignalRankings;

public sealed class GetSignalRankingsTool(
    ILogger<GetSignalRankingsTool> logger,
    IMessageBus bus,
    IOptions<PlutusOptions> options
) : AssistantDataTool<IMarketScope, SignalRankingsArguments, SignalRankingsData>(logger)
{
    internal const string NoSignals = "No signals have been computed for this market.";

    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IOptions<PlutusOptions> _options = Guard.Against.Null(options);

    public override string Name => "get_signal_rankings";

    public override string Description =>
        "Symbols with the highest signal score, overall or for one intent (Buy, Sell, Flip, "
        + "Merch). Scores range from -1 to 1.";

    protected override async ValueTask<AssistantData<SignalRankingsData>> QueryAsync(
        IMarketScope context,
        SignalRankingsArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var sortField = arguments.Intent switch
        {
            SignalIntent.Buy => nameof(GetSignalRankingsResponse.BuyScore),
            SignalIntent.Sell => nameof(GetSignalRankingsResponse.SellScore),
            SignalIntent.Flip => nameof(GetSignalRankingsResponse.FlipScore),
            SignalIntent.Merch => nameof(GetSignalRankingsResponse.MerchScore),
            _ => nameof(GetSignalRankingsResponse.OverallScore),
        };

        var rankings = await _bus.InvokeAsync<PagedResponse<GetSignalRankingsResponse>>(
            new GetSignalRankingsInput(
                context.MarketId,
                sortField,
                SortDirection.Desc.ToQueryValue(),
                Take: _options.Value.AssistantTools.ClampRows(arguments.Take)
            ),
            cancellationToken
        );

        if (!rankings.Items.Any())
        {
            return AssistantData<SignalRankingsData>.WithNote(NoSignals);
        }

        return new SignalRankingsData([
            .. rankings.Items.Select(row => new SignalRankingData(
                row.SymbolId,
                row.SymbolName,
                Score(row, arguments.Intent),
                row.BullishCount,
                row.BearishCount,
                row.DailyAveragePrice,
                row.DailyVolume
            )),
        ]);
    }

    private static decimal? Score(GetSignalRankingsResponse row, SignalIntent? intent)
    {
        return intent switch
        {
            SignalIntent.Buy => row.BuyScore,
            SignalIntent.Sell => row.SellScore,
            SignalIntent.Flip => row.FlipScore,
            SignalIntent.Merch => row.MerchScore,
            _ => row.OverallScore,
        };
    }
}
