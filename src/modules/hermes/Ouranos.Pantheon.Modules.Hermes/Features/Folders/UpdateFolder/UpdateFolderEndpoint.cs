using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder;

public static class UpdateFolderEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/folders/{folderId}", Handle).WithTags("Hermes.Folders");
    }

    internal static async Task<IResult> Handle(
        Id<Folder> folderId,
        UpdateFolderBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new UpdateFolderInput(folderId, body.Name, body.IsPublic, body.ParentFolderId);
        return Results.Ok(await bus.InvokeAsync<IdResponse<Folder>>(input, ct));
    }
}
