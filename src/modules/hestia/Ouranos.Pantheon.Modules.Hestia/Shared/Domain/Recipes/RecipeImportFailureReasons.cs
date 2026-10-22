namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;

/// <summary>User-facing reasons for import failures recorded outside the import consumer.</summary>
public static class RecipeImportFailureReasons
{
    public const string NotQueued = "The import could not be queued. Try again.";
    public const string Stale = "The import did not finish in time. Try again.";
}
