using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona;

public static class UpdatePersonaEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/personas/{personaId}", Handle).WithTags("Hermes.Personas");
    }

    internal static async Task<IResult> Handle(
        Id<Persona> personaId,
        UpdatePersonaBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new UpdatePersonaInput(
            personaId,
            body.Name,
            body.Description,
            body.Personality,
            body.Scenario,
            body.IsDefault,
            body.IsPublic
        );
        return Results.Ok(await bus.InvokeAsync<UpdatePersonaResponse>(input, ct));
    }
}
