using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

public static class TraitValidation
{
    /// <summary>
    /// Loads the selected traits, rejecting any id that doesn't exist rather than dropping it.
    /// </summary>
    public static async Task<List<Trait>> LoadByIdsAsync(
        HermesDbContext dbContext,
        Id<Trait>[]? traitIds,
        CancellationToken cancellationToken = default
    )
    {
        if (traitIds is not { Length: > 0 })
        {
            return [];
        }

        var traits = await dbContext
            .Traits.Where(t => traitIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var traitId in traitIds)
        {
            Guard.Against.NotFound(traitId, traits.FirstOrDefault(t => t.Id == traitId), "trait");
        }

        return traits;
    }
}
