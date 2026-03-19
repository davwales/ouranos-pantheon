using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.DeleteAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.DeleteAssistant;

public static class DeleteAssistantEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hermes/assistants/{assistantId}", Handle)
            .WithTags("Hermes.Assistants");
    }

    private static async Task<IResult> Handle(
        Id<Assistant> assistantId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<IdResponse<Assistant>>(new DeleteAssistantInput(assistantId), ct));
    }
}
