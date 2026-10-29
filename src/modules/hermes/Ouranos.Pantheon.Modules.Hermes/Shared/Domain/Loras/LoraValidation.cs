using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;

public static class LoraValidation
{
    /// <summary>
    /// Each image-server LoRA has at most one config, so a conversation can't apply it twice.
    /// </summary>
    public static async Task ValidateIdentifierIsFreeAsync(
        HermesDbContext dbContext,
        string identifier,
        Id<Lora>? loraId,
        CancellationToken cancellationToken = default
    )
    {
        var trimmed = identifier.Trim();
        var isTaken = await dbContext.Loras.AnyAsync(
            l => l.Identifier == trimmed && l.Id != loraId,
            cancellationToken
        );

        if (isTaken)
        {
            throw new ConflictException($"The LoRA '{trimmed}' already has a config.");
        }
    }

    /// <summary>
    /// Loads the selected LoRAs, rejecting any id that doesn't exist rather than dropping it.
    /// </summary>
    public static async Task<List<Lora>> LoadByIdsAsync(
        HermesDbContext dbContext,
        Id<Lora>[]? loraIds,
        CancellationToken cancellationToken = default
    )
    {
        if (loraIds is not { Length: > 0 })
        {
            return [];
        }

        var loras = await dbContext
            .Loras.Where(l => loraIds.Contains(l.Id))
            .ToListAsync(cancellationToken);

        foreach (var loraId in loraIds)
        {
            Guard.Against.NotFound(loraId, loras.FirstOrDefault(l => l.Id == loraId));
        }

        return loras;
    }
}
