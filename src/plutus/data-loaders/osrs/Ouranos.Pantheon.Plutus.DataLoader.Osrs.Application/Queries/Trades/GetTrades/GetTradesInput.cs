using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Queries.Trades.GetTrades;

public sealed record GetTradesInput : IQuery<WrapperResponse<List<GetTradesResponse>>>;