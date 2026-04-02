using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona;

public static class CreatePersonaEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/personas", Handle)
            .WithTags("Hermes.Personas");
    }

    internal static async Task<IResult> Handle(
        CreatePersonaInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CreatePersonaResponse>(input, ct);
        return Results.Created($"/api/hermes/personas/{result.PersonaId}", result);
    }
}
