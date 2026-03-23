using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;

public sealed record UpdatePersonaResponse(
    Id<Persona> PersonaId
);
