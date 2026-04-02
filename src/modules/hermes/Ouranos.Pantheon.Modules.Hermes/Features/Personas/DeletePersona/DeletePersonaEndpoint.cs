using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona;

public static class DeletePersonaEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hermes/personas/{personaId}", Handle)
            .WithTags("Hermes.Personas");
    }

    internal static async Task<IResult> Handle(
        Id<Persona> personaId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<DeletePersonaResponse>(new DeletePersonaInput(personaId), ct));
    }
}
