using System.Globalization;
using System.Text.Json;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools;

/// <summary>
/// Turns the conditions a model asked for into a market-trades filter, collecting readable
/// problems for anything it cannot honour so the model can correct itself.
/// </summary>
internal static class MarketTradesFilter
{
    private static readonly MarketTradesField[] SortableFields =
    [
        MarketTradesField.TotalGain,
        MarketTradesField.Margin,
        MarketTradesField.AveragePrice,
        MarketTradesField.Roi,
        MarketTradesField.MinPrice,
        MarketTradesField.MaxPrice,
        MarketTradesField.TotalVolume,
    ];

    // These delimit conditions and groups in the filter syntax, so they cannot appear in values.
    private static readonly char[] ReservedCharacters = [':', '|', '(', ')', ','];

    private const string PlainNumberFormat = "0.############################";

    private static readonly (string Suffix, decimal Multiplier)[] Multipliers =
    [
        ("k", 1_000m),
        ("m", 1_000_000m),
        ("b", 1_000_000_000m),
    ];

    public static MarketTradesQuery Build(
        IReadOnlyList<MarketTradesCondition>? conditions,
        MarketTradesField? sortBy
    )
    {
        var problems = new List<string>();
        var valid = new List<ExplorerCondition>();
        var filter = new List<string>();

        foreach (var condition in conditions ?? [])
        {
            var problem = Validate(condition, out var explorerCondition);
            if (problem is null)
            {
                valid.Add(explorerCondition);
                filter.Add(
                    FilterExpression.For(
                        explorerCondition.Field,
                        ToFilterOperator(condition.Operator),
                        explorerCondition.Value
                    )
                );
            }
            else
            {
                problems.Add(
                    $"Ignored {condition.Field} {condition.Operator} '{condition.Value}': {problem}."
                );
            }
        }

        string? sortField = null;
        if (sortBy is { } sort)
        {
            if (SortableFields.Contains(sort))
            {
                sortField = ColumnId(sort);
            }
            else
            {
                problems.Add($"Cannot sort by {sort}.");
            }
        }

        return new MarketTradesQuery(valid, [.. filter], sortField, problems);
    }

    public static string ColumnId(MarketTradesField field)
    {
        return JsonNamingPolicy.CamelCase.ConvertName(field.ToString());
    }

    private static string? Validate(MarketTradesCondition condition, out ExplorerCondition result)
    {
        result = new ExplorerCondition(
            ColumnId(condition.Field),
            FilterExpression.Operator(ToFilterOperator(condition.Operator)),
            condition.Value
        );

        var value = condition.Value.Trim();
        if (value.Length == 0)
        {
            return "no value";
        }

        if (condition.Field == MarketTradesField.SymbolName)
        {
            if (
                condition.Operator
                is not (
                    MarketTradesOperator.Eq
                    or MarketTradesOperator.Neq
                    or MarketTradesOperator.Like
                )
            )
            {
                return "names support only eq, neq and like";
            }

            if (value.IndexOfAny(ReservedCharacters) >= 0)
            {
                return "it contains a reserved character";
            }

            result = result with { Value = value };
            return null;
        }

        if (condition.Operator == MarketTradesOperator.Like)
        {
            return "like works only on SymbolName";
        }

        var isPercent = condition.Field == MarketTradesField.Roi;
        if (!TryParseNumber(isPercent ? value.TrimEnd('%') : value, out var number))
        {
            return "not a number";
        }

        var normalized = isPercent ? number / 100m : number;
        result = result with
        {
            Value = normalized.ToString(PlainNumberFormat, CultureInfo.InvariantCulture),
        };
        return null;
    }

    private static FilterOperator ToFilterOperator(MarketTradesOperator marketTradesOperator)
    {
        return marketTradesOperator switch
        {
            MarketTradesOperator.Eq => FilterOperator.Eq,
            MarketTradesOperator.Neq => FilterOperator.Neq,
            MarketTradesOperator.Gt => FilterOperator.Gt,
            MarketTradesOperator.Gte => FilterOperator.Gte,
            MarketTradesOperator.Lt => FilterOperator.Lt,
            MarketTradesOperator.Lte => FilterOperator.Lte,
            MarketTradesOperator.Like => FilterOperator.Like,
            _ => throw new ArgumentOutOfRangeException(nameof(marketTradesOperator)),
        };
    }

    private static bool TryParseNumber(string value, out decimal number)
    {
        var text = value.Replace(",", "", StringComparison.Ordinal).Replace("_", "").Trim();
        var multiplier = 1m;

        foreach (var (suffix, factor) in Multipliers)
        {
            if (text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                text = text[..^suffix.Length].Trim();
                multiplier = factor;
                break;
            }
        }

        if (
            decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
        )
        {
            number = parsed * multiplier;
            return true;
        }

        number = 0;
        return false;
    }
}
