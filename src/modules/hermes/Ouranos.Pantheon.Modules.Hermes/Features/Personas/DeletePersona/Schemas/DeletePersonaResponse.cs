using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona.Schemas;

public sealed record DeletePersonaResponse(Id<Persona> PersonaId);
