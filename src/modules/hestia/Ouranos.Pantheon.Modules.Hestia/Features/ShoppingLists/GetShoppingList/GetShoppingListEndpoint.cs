using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList;

public static class GetShoppingListEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hestia/shopping-list", Handle).WithTags("Hestia.ShoppingList");
    }

    internal static async Task<IResult> Handle(IMessageBus bus, CancellationToken ct)
    {
        return Results.Ok(
            await bus.InvokeAsync<ShoppingListResponse>(new GetShoppingListInput(), ct)
        );
    }
}
