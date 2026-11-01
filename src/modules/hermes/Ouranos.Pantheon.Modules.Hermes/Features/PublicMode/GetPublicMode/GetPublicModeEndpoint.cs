using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.PublicMode.GetPublicMode.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.PublicMode.GetPublicMode;

public static class GetPublicModeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/public-mode", Handle).WithTags("Hermes.PublicMode");
    }

    internal static async Task<IResult> Handle(IMessageBus bus, CancellationToken ct)
    {
        return Results.Ok(
            await bus.InvokeAsync<GetPublicModeResponse>(new GetPublicModeInput(), ct)
        );
    }
}
