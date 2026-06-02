using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders;

public static class GetAllFoldersEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/folders", Handle).WithTags("Hermes.Folders");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllFoldersInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<FolderSummary>>(input, ct));
    }
}
