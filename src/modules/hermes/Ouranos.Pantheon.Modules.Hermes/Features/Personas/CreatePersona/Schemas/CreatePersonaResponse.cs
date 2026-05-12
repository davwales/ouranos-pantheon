using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona.Schemas;

public sealed record CreatePersonaResponse(Id<Persona> PersonaId);
