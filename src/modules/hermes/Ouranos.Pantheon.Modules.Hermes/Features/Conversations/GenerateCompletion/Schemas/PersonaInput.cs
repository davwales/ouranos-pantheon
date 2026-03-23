namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record PersonaInput(
    string Name,
    string Description,
    string? Personality = null,
    string? Scenario = null
);
