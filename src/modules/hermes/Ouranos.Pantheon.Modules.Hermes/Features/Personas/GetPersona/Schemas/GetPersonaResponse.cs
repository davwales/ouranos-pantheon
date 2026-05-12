using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona.Schemas;

public sealed record GetPersonaResponse(
    Id<Persona> Id,
    string Name,
    string Description,
    string? Personality,
    string? Scenario,
    bool IsDefault,
    bool IsPublic
);
