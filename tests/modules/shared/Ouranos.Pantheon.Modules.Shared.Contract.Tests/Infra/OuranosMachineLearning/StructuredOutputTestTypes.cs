using System.Text.Json.Serialization;

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
