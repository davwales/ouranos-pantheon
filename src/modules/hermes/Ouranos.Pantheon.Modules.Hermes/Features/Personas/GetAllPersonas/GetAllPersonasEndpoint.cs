using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas;

public static class GetAllPersonasEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/personas", Handle)
            .WithTags("Hermes.Personas");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllPersonasInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllPersonasResponse>>(input, ct));
    }
}
