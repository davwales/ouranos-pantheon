namespace Ouranos.Pantheon.Modules.Hestia.Shared;

public sealed record HestiaOptions(
    RecipeImportOptions RecipeImport,
    KitchenAssistantOptions KitchenAssistant
)
{
    public const string SectionName = "Ouranos:Hestia";

    public HestiaOptions()
        : this(
            RecipeImport: new RecipeImportOptions(),
            KitchenAssistant: new KitchenAssistantOptions()
        ) { }
}
