using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.FindSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecast;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPositions;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignals;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst;

public static class SymbolAnalystToolFactory
{
    public static IReadOnlyList<IAssistantTool<SymbolAnalystContext>> All(
        IMessageBus? bus = null,
        PlutusOptions? options = null
    )
    {
        bus ??= Substitute.For<IMessageBus>();
        var plutusOptions = Options.Create(options ?? new PlutusOptions());

        return
        [
            new GetPriceHistoryTool(
                Substitute.For<ILogger<GetPriceHistoryTool>>(),
                bus,
                plutusOptions
            ),
            new GetSignalsTool(Substitute.For<ILogger<GetSignalsTool>>(), bus),
            new GetForecastTool(Substitute.For<ILogger<GetForecastTool>>(), bus),
            new GetPositionsTool(Substitute.For<ILogger<GetPositionsTool>>(), bus, plutusOptions),
            new FindSymbolsTool(Substitute.For<ILogger<FindSymbolsTool>>(), bus, plutusOptions),
            new GetSymbolTool(Substitute.For<ILogger<GetSymbolTool>>(), bus),
        ];
    }
}
