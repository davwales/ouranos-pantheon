using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Assistants.UpdateAssistant;

public sealed record UpdateAssistantInput(
    Id<Assistant> AssistantId,
    string Model,
    string SystemPrompt,
    string? AssistantName = null,
    string? UserName = null
) : ICommand<IdResponse<Assistant>>;