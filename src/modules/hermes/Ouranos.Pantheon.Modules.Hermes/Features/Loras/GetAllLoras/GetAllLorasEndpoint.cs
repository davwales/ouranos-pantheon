using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras;

public static class GetAllLorasEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/loras", Handle).WithTags("Hermes.Loras");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllLorasInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllLorasResponse>>(input, ct));
    }
}
