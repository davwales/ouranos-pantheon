using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;

public sealed record RecipeCreated(
    Guid Id,
    string Title,
    string? SourceUrl,
    string Instructions,
    List<Ingredient> Ingredients,
    string Notes,
    DateTimeOffset CreatedAt
) : IDomainEvent;
