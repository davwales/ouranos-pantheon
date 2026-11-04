using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;
using Ouranos.Pantheon.Modules.Shared.Contract.API;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;

public static class GenerateCompletionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/conversations/completions/stream", Handle)
            .WithTags("Hermes.Conversations")
            .DisabledInPublicMode<GenerateCompletionInput>(input =>
                input.ConversationId is not null
            );
    }

    private static async Task Handle(
        GenerateCompletionInput input,
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        var stream = await bus.InvokeAsync<IAsyncEnumerable<AssistantEvent>>(input, ct);
        await SseWriter.WriteStreamAsync(httpContext.Response, stream, ct);
    }
}
