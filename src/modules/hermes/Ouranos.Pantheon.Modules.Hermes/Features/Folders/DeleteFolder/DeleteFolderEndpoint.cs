using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder;

public static class DeleteFolderEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hermes/folders/{folderId}", Handle).WithTags("Hermes.Folders");
    }

    internal static async Task<IResult> Handle(
        Id<Folder> folderId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<IdResponse<Folder>>(new DeleteFolderInput(folderId), ct)
        );
    }
}
