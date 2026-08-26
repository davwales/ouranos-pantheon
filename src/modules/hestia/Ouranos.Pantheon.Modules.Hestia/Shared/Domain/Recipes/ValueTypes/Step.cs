using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

public sealed record Step
{
    public string Text { get; init; } = string.Empty;

    public Step(string text)
    {
        Guard.Against.NullOrWhiteSpace(text);
        Guard.Against.OutOfRange(text.Length, nameof(text), 1, 2000);
        Text = text;
    }
}
