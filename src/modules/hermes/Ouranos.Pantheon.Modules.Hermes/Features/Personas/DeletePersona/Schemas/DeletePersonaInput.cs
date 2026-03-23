using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona.Schemas;

public sealed record DeletePersonaInput(
    Id<Persona> PersonaId
);
