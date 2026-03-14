using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant.Schemas;

public sealed record CreateAssistantInput(
    string Model,
    string SystemPrompt,
    string? AssistantName = null,
    string? UserName = null,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null
) : ICommand<IdResponse<Assistant>>;