using MediatR;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Domain.Markets;
using Talos.Olympus.Service.Plutus.Domain.Symbols;

namespace Talos.Olympus.Service.Plutus.Application.Queries.Trades.GetSymbolTrades;

public sealed record GetSymbolTradesInput(
    Id<Symbol> SymbolId,
    Id<Market> MarketId,
    int NumBuckets = 100,
    double? Seconds = null
) : IRequest<GetSymbolTradesResponse>;