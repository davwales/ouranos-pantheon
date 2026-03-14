using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant.Schemas;

public sealed record GetAssistantInput(
    Id<Assistant> AssistantId
) : IQuery<Assistant>;
