namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory.Schemas;

public sealed record RecipeHistoryEventResponse(
    long Version,
    string EventType,
    DateTimeOffset Timestamp
);
