namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;

public sealed record UpdatePersonaBody(
    string Name,
    string Description,
    string? Personality = null,
    string? Scenario = null,
    bool IsDefault = false,
    bool IsPublic = true
);
