using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras;

public static class GetAvailableLorasEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/available-loras", Handle).WithTags("Hermes.Loras");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAvailableLorasInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAvailableLorasResponse>>(input, ct));
    }
}
