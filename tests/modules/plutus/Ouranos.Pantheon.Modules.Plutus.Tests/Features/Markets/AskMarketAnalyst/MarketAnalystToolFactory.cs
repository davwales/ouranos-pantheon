using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.FindSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetActivityHours;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetForecastLeaders;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSignalRankings;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.SetExplorerView;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst;

public static class MarketAnalystToolFactory
{
    public static IReadOnlyList<IAssistantTool<MarketAnalystContext>> All(IMessageBus? bus = null)
    {
        bus ??= Substitute.For<IMessageBus>();
        var options = Options.Create(new PlutusOptions());

        return
        [
            new FindSymbolsTool(Substitute.For<ILogger<FindSymbolsTool>>(), bus, options),
            new GetSymbolTool(Substitute.For<ILogger<GetSymbolTool>>(), bus),
            new GetSignalRankingsTool(
                Substitute.For<ILogger<GetSignalRankingsTool>>(),
                bus,
                options
            ),
            new GetForecastLeadersTool(
                Substitute.For<ILogger<GetForecastLeadersTool>>(),
                bus,
                options
            ),
            new GetActivityHoursTool(Substitute.For<ILogger<GetActivityHoursTool>>(), bus),
            new SetExplorerViewTool(Substitute.For<ILogger<SetExplorerViewTool>>(), bus),
        ];
    }
}
