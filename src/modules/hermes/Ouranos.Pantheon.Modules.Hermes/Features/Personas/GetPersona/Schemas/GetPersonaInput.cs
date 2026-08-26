using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona.Schemas;

public sealed record GetPersonaInput(Id<Persona> PersonaId);
