using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder;

public static class GetFolderEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/folders/{folderId}", Handle).WithTags("Hermes.Folders");
    }

    internal static async Task<IResult> Handle(
        Id<Folder> folderId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<GetFolderResponse>(new GetFolderInput(folderId), ct)
        );
    }
}
