using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;

public sealed record GetAllMarketsInput : IQuery<WrapperResponse<IQueryable<Market>>>;
