namespace Ouranos.Pantheon.Modules.Hestia.Shared;

public sealed record HestiaOptions(RecipeImportOptions RecipeImport)
{
    public const string SectionName = "Ouranos:Hestia";

    public HestiaOptions()
        : this(RecipeImport: new RecipeImportOptions()) { }
}
