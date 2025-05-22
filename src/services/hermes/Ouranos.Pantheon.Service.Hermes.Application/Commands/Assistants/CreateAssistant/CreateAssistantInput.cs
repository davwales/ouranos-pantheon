using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Assistants.CreateAssistant;

public sealed record CreateAssistantInput(
    string Model,
    string SystemPrompt,
    string? AssistantName = null,
    string? UserName = null
) : ICommand<IdResponse<Assistant>>;