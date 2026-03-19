using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant;

public static class UpdateAssistantEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/assistants/{assistantId}", Handle)
            .WithTags("Hermes.Assistants");
    }

    private static async Task<IResult> Handle(
        UpdateAssistantInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<IdResponse<Assistant>>(input, ct));
    }
}
