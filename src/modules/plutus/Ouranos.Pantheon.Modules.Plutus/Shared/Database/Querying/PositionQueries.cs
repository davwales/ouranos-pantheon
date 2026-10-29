using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database.Querying;

public static class PositionQueries
{
    /// <summary>
    /// Quantity of a buy position already covered by its linked sells, excluding one sell (the one
    /// being linked or modified). A sell that did not sell leaves its quantity available for a retry.
    /// </summary>
    public static Task<decimal> CommittedSellQuantityAsync(
        this IQueryable<Position> positions,
        Id<Position> buyPositionId,
        Id<Position>? excludedSellPositionId,
        CancellationToken cancellationToken
    )
    {
        var linkedSells = positions.Where(p =>
            p.LinkedBuyPositionId == buyPositionId && p.Status != PositionStatus.DidNotSell
        );

        if (excludedSellPositionId is { } excludedId)
        {
            linkedSells = linkedSells.Where(p => p.Id != excludedId);
        }

        return linkedSells.SumAsync(p => p.Quantity, cancellationToken);
    }
}
