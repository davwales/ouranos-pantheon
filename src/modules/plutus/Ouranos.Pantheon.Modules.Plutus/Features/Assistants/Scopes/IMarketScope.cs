using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;

/// <summary>
/// What a tool needs from an assistant's context to work within one market. Tools are typed to
/// the narrowest scope they need, so any assistant whose context implements it can register them.
/// </summary>
public interface IMarketScope
{
    Id<Market> MarketId { get; }

    TimeFrame TimeFrame { get; }
}
