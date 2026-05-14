namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;

public sealed record UpdateTraitBody(string Name, string Content, bool IsPublic = true);
