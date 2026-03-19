using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant.Schemas;

public sealed record GetAssistantInput(
    Id<Assistant> AssistantId
);
