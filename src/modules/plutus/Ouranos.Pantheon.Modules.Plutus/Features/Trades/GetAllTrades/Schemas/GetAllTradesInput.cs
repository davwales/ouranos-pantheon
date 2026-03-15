using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;

public sealed record GetAllTradesInput : IQuery<WrapperResponse<IQueryable<Trade>>>;
