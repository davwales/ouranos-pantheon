using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;

public sealed record GetConversationPersonaResponse(
    Id<Persona> Id,
    string Name,
    string Description,
    string? Personality,
    string? Scenario
);
