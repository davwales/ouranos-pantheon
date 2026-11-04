using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags;

public static class FeatureFlagsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/feature-flags", Handle)
            .WithTags("FeatureFlags")
            .Produces<GetFeatureFlagsResponse>(StatusCodes.Status200OK);
    }

    internal static async Task<IResult> Handle(IMessageBus bus, CancellationToken ct)
    {
        return Results.Ok(
            await bus.InvokeAsync<GetFeatureFlagsResponse>(new GetFeatureFlagsRequest(), ct)
        );
    }
}
