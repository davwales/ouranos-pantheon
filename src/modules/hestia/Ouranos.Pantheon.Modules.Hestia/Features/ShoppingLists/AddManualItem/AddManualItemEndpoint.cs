using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem;

public static class AddManualItemEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hestia/shopping-list/manual-items", Handle)
            .WithTags("Hestia.ShoppingList");
    }

    internal static async Task<IResult> Handle(
        AddManualItemBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<ManualItemResponse>(
            new AddManualItemInput(body.Text),
            ct
        );
        return Results.Ok(result);
    }
}
