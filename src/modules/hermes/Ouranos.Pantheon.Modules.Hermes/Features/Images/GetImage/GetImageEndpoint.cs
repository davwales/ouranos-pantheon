using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage;

public static class GetImageEndpoint
{
    // Images never change once generated, so the browser may reuse them for a day; private keeps
    // shared caches from holding a conversation's images.
    private const string CacheControl = "private, max-age=86400";

    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/images/{imageId}", Handle).WithTags("Hermes.Images");
    }

    internal static async Task<IResult> Handle(
        Id<GeneratedImage> imageId,
        HttpResponse response,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var image = await bus.InvokeAsync<GetImageResponse>(new GetImageInput(imageId), ct);
        response.Headers.CacheControl = CacheControl;
        return Results.File(image.Content, image.MediaType);
    }
}
