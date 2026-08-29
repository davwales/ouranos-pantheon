using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem;

public static class DeleteManualItemEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hestia/shopping-list/manual-items/{itemId}", Handle)
            .WithTags("Hestia.ShoppingList");
    }

    internal static async Task<IResult> Handle(Guid itemId, IMessageBus bus, CancellationToken ct)
    {
        return Results.Ok(
            await bus.InvokeAsync<DeleteManualItemResponse>(new DeleteManualItemInput(itemId), ct)
        );
    }
}
