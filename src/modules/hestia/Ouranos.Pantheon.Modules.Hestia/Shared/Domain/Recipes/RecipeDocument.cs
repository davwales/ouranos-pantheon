using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;

public sealed class RecipeDocument
{
    public Guid Id { get; set; }

    [JsonIgnore]
    public Id<RecipeDocument> RecipeId => new(Id.ToString());

    public string Title { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public string Instructions { get; set; } = string.Empty;

    public List<Ingredient> Ingredients { get; set; } = [];

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
