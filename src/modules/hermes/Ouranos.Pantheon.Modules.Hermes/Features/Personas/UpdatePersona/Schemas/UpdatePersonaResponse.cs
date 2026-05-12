using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;

public sealed record UpdatePersonaResponse(Id<Persona> PersonaId);
