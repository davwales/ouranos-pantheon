using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems;

public static class UpdateCheckedItemsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hestia/shopping-list/checked-items", Handle)
            .WithTags("Hestia.ShoppingList");
    }

    internal static async Task<IResult> Handle(
        UpdateCheckedItemsBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<UpdateCheckedItemsResponse>(
            new UpdateCheckedItemsInput(body.CheckedItemIds),
            ct
        );
        return Results.Ok(result);
    }
}
