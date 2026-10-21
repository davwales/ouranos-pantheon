using System.ComponentModel;
using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Infra.OuranosMachineLearning;

internal sealed record TestIngredient
{
    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}

internal sealed record TestDocument
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("ingredients")]
    public List<TestIngredient> Ingredients { get; init; } = [];

    [JsonPropertyName("steps")]
    public List<string> Steps { get; init; } = [];
}

internal sealed record TestIdArguments(
    Id<TestIngredient> IngredientId,
    Id<TestIngredient>? ParentId
);

[Description("How the ingredient is prepared.")]
internal enum TestPreparation
{
    Raw,
    Cooked,
}

internal sealed record TestDescribedArguments(
    [Description("The ingredient name.")] string Name,
    string? Note,
    [Description("The ingredient to replace.")] Id<TestIngredient> IngredientId,
    [Description("The parent ingredient, if any.")] Id<TestIngredient>? ParentId,
    TestPreparation Preparation
)
{
    [property: Description("Ignored by the exporter otherwise.")]
    public int Servings { get; init; }
}
