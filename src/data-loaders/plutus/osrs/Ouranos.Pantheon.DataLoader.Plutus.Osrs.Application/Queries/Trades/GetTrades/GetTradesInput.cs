using MediatR;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

public sealed record GetTradesInput : IRequest<List<GetTradesResponse>>;