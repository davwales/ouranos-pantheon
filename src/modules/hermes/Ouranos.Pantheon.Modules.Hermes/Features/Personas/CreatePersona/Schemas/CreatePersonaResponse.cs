using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona.Schemas;

public sealed record CreatePersonaResponse(
    Id<Persona> PersonaId
);
