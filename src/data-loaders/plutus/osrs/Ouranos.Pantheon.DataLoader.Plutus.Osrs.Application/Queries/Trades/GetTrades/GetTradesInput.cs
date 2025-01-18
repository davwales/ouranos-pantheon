using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

public sealed record GetTradesInput : IQuery<WrapperResponse<List<GetTradesResponse>>>;