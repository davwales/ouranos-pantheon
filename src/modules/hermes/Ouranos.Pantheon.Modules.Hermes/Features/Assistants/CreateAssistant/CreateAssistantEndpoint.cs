using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant;

public static class CreateAssistantEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/assistants", Handle)
            .WithTags("Hermes.Assistants");
    }

    private static async Task<IResult> Handle(
        CreateAssistantInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<Assistant>>(input, ct);
        return Results.Created($"/api/hermes/assistants/{result.Id}", result);
    }
}
