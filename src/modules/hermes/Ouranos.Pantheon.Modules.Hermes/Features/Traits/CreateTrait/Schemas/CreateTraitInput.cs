namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait.Schemas;

public sealed record CreateTraitInput(string Name, string Content, bool IsPublic = true);
