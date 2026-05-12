using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits;

public static class GetAllTraitsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/traits", Handle).WithTags("Hermes.Traits");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllTraitsInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllTraitsResponse>>(input, ct));
    }
}
