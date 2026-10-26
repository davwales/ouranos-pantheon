using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails;

public static class GetImageDetailsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/images/{imageId}/details", Handle).WithTags("Hermes.Images");
    }

    internal static async Task<IResult> Handle(
        Id<GeneratedImage> imageId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetImageDetailsResponse>(
            new GetImageDetailsInput(imageId),
            ct
        );
        return Results.Ok(result);
    }
}
