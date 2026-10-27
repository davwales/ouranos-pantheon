using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Shared.Contract.API;

public static class AssistantEndpointExtensions
{
    /// <summary>
    /// Maps a POST endpoint that streams a <see cref="PantheonAssistant{TContext}"/> completion
    /// as Server-Sent Events. The request body binds to a closed
    /// <see cref="AssistantCompletionInput{TContext}"/>, so each assistant exposes its own typed
    /// context schema.
    /// </summary>
    public static RouteHandlerBuilder MapAssistant<TContext>(
        this IEndpointRouteBuilder app,
        string pattern
    )
        where TContext : class
    {
        return app.MapPost(pattern, HandleAsync<TContext>);
    }

    internal static async Task HandleAsync<TContext>(
        AssistantCompletionInput<TContext> input,
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken ct
    )
        where TContext : class
    {
        var stream = await bus.InvokeAsync<IAsyncEnumerable<AssistantEvent>>(input, ct);
        await SseWriter.WriteStreamAsync(httpContext.Response, stream, ct);
    }
}
