using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.API;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;

public static class AskKitchenAssistantEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapAssistant<KitchenAssistantContext>(
                "/api/hestia/recipes/assistant/completions/stream"
            )
            .WithTags("Hestia.Recipes");
    }
}
