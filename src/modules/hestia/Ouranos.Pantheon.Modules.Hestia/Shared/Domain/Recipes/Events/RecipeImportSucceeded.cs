using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;

public sealed record RecipeImportSucceeded(
    string Title,
    List<Step> Steps,
    List<Ingredient> Ingredients,
    string Notes,
    DateTimeOffset ImportedAt
) : IDomainEvent;
