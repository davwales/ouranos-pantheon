using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Messages;

public sealed record UpsertSymbolMessage(
    Id<Market> MarketId,
    string SymbolCode,
    string? SymbolSubcode,
    string SymbolName,
    AdditionalFields AdditionalFields
) : ICommand<Symbol>;