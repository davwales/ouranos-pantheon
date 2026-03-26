using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas.Schemas;

public sealed record GetAllPersonasResponse(
    Id<Persona> Id,
    string Name,
    string Description,
    string? Personality,
    string? Scenario,
    bool IsDefault,
    bool IsPublic
);
