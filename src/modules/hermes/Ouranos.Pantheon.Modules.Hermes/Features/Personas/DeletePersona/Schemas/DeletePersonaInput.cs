using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona.Schemas;

public sealed record DeletePersonaInput(Id<Persona> PersonaId);
