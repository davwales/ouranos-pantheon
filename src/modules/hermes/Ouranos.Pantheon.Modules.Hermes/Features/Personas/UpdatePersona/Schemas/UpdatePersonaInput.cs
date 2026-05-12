using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;

public sealed record UpdatePersonaInput(
    Id<Persona> PersonaId,
    string Name,
    string Description,
    string? Personality = null,
    string? Scenario = null,
    bool IsDefault = false,
    bool IsPublic = true
);
