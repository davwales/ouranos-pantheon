using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Hermes.Service.Domain.Assistants;

namespace Ouranos.Pantheon.Hermes.Service.Application.Commands.Assistants.UpdateAssistant;

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