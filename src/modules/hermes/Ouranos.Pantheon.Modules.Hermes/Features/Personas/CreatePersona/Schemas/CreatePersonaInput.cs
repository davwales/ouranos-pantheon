namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona.Schemas;

public sealed record CreatePersonaInput(
    string Name,
    string Description,
    string? Personality = null,
    string? Scenario = null,
    bool IsDefault = false
);
