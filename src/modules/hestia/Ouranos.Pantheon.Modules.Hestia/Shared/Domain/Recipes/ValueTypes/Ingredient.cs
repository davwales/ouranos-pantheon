using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

public sealed record Ingredient
{
    public decimal Quantity { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    public Ingredient(decimal quantity, string unit, string name)
    {
        Guard.Against.OutOfRange(quantity, nameof(quantity), 0, decimal.MaxValue);
        Guard.Against.NullOrWhiteSpace(unit);
        Guard.Against.OutOfRange(unit.Length, nameof(unit), 1, 50);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.OutOfRange(name.Length, nameof(name), 1, 200);

        Quantity = quantity;
        Unit = unit;
        Name = name;
    }
}
