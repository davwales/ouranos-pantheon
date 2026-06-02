using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder;

public static class CreateFolderEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/folders", Handle).WithTags("Hermes.Folders");
    }

    internal static async Task<IResult> Handle(
        CreateFolderInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CreateFolderResponse>(input, ct);
        return Results.Created($"/api/hermes/folders/{result.FolderId}", result);
    }
}
