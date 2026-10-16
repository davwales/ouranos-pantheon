using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskMarketAnalyst.Schemas;

/// <summary>
/// <see cref="View"/> is set only on the explorer, where the assistant can also change the table.
/// </summary>
public sealed record MarketAnalystContext(
    Id<Market> MarketId,
    TimeFrame TimeFrame,
    ExplorerView? View = null
) : IExplorerScope;
