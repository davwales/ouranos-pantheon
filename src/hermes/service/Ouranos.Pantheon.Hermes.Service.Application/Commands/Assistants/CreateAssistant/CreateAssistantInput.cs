using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Hermes.Service.Domain.Assistants;

namespace Ouranos.Pantheon.Hermes.Service.Application.Commands.Assistants.CreateAssistant;

public sealed record CreateAssistantInput(
    string Model,
    string SystemPrompt,
    string? AssistantName = null,
    string? UserName = null,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null
) : ICommand<IdResponse<Assistant>>;