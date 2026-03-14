using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant.Schemas;

public sealed record UpdateAssistantInput(
    Id<Assistant> AssistantId,
    string Model,
    string SystemPrompt,
    string? AssistantName = null,
    string? UserName = null,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null
) : ICommand<IdResponse<Assistant>>;
