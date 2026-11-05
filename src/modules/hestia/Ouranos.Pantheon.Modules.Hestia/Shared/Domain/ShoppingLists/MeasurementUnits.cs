namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

public static class MeasurementUnits
{
    private static readonly MeasurementUnit Teaspoon = new("tsp", UnitDimension.Volume, 4.92892m);
    private static readonly MeasurementUnit Tablespoon = new(
        "tbsp",
        UnitDimension.Volume,
        14.7868m
    );
    private static readonly MeasurementUnit FluidOunce = new(
        "fl oz",
        UnitDimension.Volume,
        29.5735m
    );
    private static readonly MeasurementUnit Cup = new("cup", UnitDimension.Volume, 236.588m);
    private static readonly MeasurementUnit Pint = new("pint", UnitDimension.Volume, 473.176m);
    private static readonly MeasurementUnit Quart = new("quart", UnitDimension.Volume, 946.353m);
    private static readonly MeasurementUnit Gallon = new("gallon", UnitDimension.Volume, 3785.41m);
    private static readonly MeasurementUnit Millilitre = new("ml", UnitDimension.Volume, 1m);
    private static readonly MeasurementUnit Litre = new("l", UnitDimension.Volume, 1000m);
    private static readonly MeasurementUnit Milligram = new("mg", UnitDimension.Mass, 0.001m);
    private static readonly MeasurementUnit Gram = new("g", UnitDimension.Mass, 1m);
    private static readonly MeasurementUnit Kilogram = new("kg", UnitDimension.Mass, 1000m);
    private static readonly MeasurementUnit Ounce = new("oz", UnitDimension.Mass, 28.3495m);
    private static readonly MeasurementUnit Pound = new("lb", UnitDimension.Mass, 453.592m);

    // Singular spellings only; Find strips a plural "s" before looking up.
    private static readonly Dictionary<string, MeasurementUnit> BySpelling = new(
        StringComparer.Ordinal
    )
    {
        ["tsp"] = Teaspoon,
        ["teaspoon"] = Teaspoon,
        ["tbsp"] = Tablespoon,
        ["tbs"] = Tablespoon,
        ["tablespoon"] = Tablespoon,
        ["fl oz"] = FluidOunce,
        ["fluid ounce"] = FluidOunce,
        ["cup"] = Cup,
        ["pint"] = Pint,
        ["pt"] = Pint,
        ["quart"] = Quart,
        ["qt"] = Quart,
        ["gallon"] = Gallon,
        ["gal"] = Gallon,
        ["ml"] = Millilitre,
        ["millilitre"] = Millilitre,
        ["milliliter"] = Millilitre,
        ["l"] = Litre,
        ["litre"] = Litre,
        ["liter"] = Litre,
        ["mg"] = Milligram,
        ["milligram"] = Milligram,
        ["g"] = Gram,
        ["gram"] = Gram,
        ["kg"] = Kilogram,
        ["kilogram"] = Kilogram,
        ["oz"] = Ounce,
        ["ounce"] = Ounce,
        ["lb"] = Pound,
        ["pound"] = Pound,
    };

    public static MeasurementUnit? Find(string unit)
    {
        var spelling = ShoppingListNormalizer.Normalize(unit.Replace(".", string.Empty));
        if (BySpelling.TryGetValue(spelling, out var match))
        {
            return match;
        }

        return spelling.EndsWith('s') && BySpelling.TryGetValue(spelling[..^1], out var plural)
            ? plural
            : null;
    }
}
