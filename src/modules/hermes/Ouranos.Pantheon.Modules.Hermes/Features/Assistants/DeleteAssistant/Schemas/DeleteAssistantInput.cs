using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.DeleteAssistant.Schemas;

public sealed record DeleteAssistantInput(
    Id<Assistant> AssistantId
);
