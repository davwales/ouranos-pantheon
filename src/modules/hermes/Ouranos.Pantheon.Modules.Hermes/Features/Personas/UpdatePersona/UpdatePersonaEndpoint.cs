using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona;

public static class UpdatePersonaEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/personas/{personaId}", Handle).WithTags("Hermes.Personas");
    }

    internal static async Task<IResult> Handle(
        UpdatePersonaInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<UpdatePersonaResponse>(input, ct));
    }
}
