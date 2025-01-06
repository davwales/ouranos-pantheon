using MediatR;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Trades.GetSymbolTrades;

public sealed record GetSymbolTradesInput(
    Id<Symbol> SymbolId,
    Id<Market> MarketId,
    int NumBuckets = 100,
    double? Seconds = null
) : IRequest<GetSymbolTradesResponse>;