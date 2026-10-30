using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory;

public static class SetItemCategoryEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hestia/shopping-list/categories", Handle).WithTags("Hestia.ShoppingList");
    }

    internal static async Task<IResult> Handle(
        SetItemCategoryBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<SetItemCategoryResponse>(
            new SetItemCategoryInput(body.Name, body.Category),
            ct
        );
        return Results.Ok(result);
    }
}
