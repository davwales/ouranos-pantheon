using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel;

public static class UpdateModelEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/models/{modelId}", Handle).WithTags("Hermes.Models");
    }

    internal static async Task<IResult> Handle(
        Id<ModelConfig> modelId,
        UpdateModelBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new UpdateModelInput(
            modelId,
            body.Name,
            body.ModelIdentifier,
            body.SystemPrompt,
            body.Temperature,
            body.MaxTokens,
            body.RepeatPenalty,
            body.ContextWindow,
            body.IsDefault,
            body.IsPublic
        );
        return Results.Ok(await bus.InvokeAsync<UpdateModelResponse>(input, ct));
    }
}
