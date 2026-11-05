using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems;

public static class ClearCheckedItemsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hestia/shopping-list/checked-items", Handle)
            .WithTags("Hestia.ShoppingList");
    }

    internal static async Task<IResult> Handle(IMessageBus bus, CancellationToken ct)
    {
        return Results.Ok(
            await bus.InvokeAsync<ClearCheckedItemsResponse>(new ClearCheckedItemsInput(), ct)
        );
    }
}
