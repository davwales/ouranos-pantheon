using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;

public static class PersonaValidation
{
    public static async Task ValidatePersonaExistsAsync(
        HermesDbContext dbContext,
        Id<Persona> personaId,
        CancellationToken cancellationToken = default
    )
    {
        var exists = await dbContext.Personas.AnyAsync(p => p.Id == personaId, cancellationToken);

        if (!exists)
        {
            Guard.Against.NotFound(personaId, (Persona?)null, "persona");
        }
    }

    public static async Task ValidatePersonaIsUnusedAsync(
        HermesDbContext dbContext,
        Persona persona,
        CancellationToken cancellationToken = default
    )
    {
        var conversationCount = await dbContext.Conversations.CountAsync(
            c => c.PersonaId == persona.Id,
            cancellationToken
        );

        if (conversationCount > 0)
        {
            throw new ConflictException(
                $"The persona '{persona.Name}' is used by {conversationCount} "
                    + $"conversation{(conversationCount == 1 ? string.Empty : "s")}. "
                    + "Switch them to another persona or delete them first."
            );
        }
    }
}
