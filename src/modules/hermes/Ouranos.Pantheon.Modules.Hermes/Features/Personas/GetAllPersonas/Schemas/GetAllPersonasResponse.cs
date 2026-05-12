using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;

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
