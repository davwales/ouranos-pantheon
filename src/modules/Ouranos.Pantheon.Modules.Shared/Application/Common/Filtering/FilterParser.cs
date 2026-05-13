using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

/// <summary>
/// Parses filter expression strings into a <see cref="FilterNode"/> AST.
///
/// Groups may be nested arbitrarily. Pipe characters inside nested groups are
/// not treated as sibling separators (depth-aware splitting).
/// </summary>
/// <example>
/// Leaf:      field:op:value   e.g. price:gt:100, name:like:sword, status:null
/// And group: and(expr|expr|...)
/// Or group:  or(expr|expr|...)
/// </example>
public static class FilterParser
{
    private const char ComponentSeparator = ':';
    private const char GroupSeparator = '|';
    private const char GroupStart = '(';
    private const char GroupEnd = ')';

    public static FilterNode Parse(string filter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);
        return ParseNode(filter.Trim());
    }

    private static FilterNode ParseNode(string s)
    {
        foreach (var logicalOperator in Enum.GetValues<LogicalOperator>())
        {
            var startKey = $"{logicalOperator}{GroupStart}";
            var endKey = $"{GroupEnd}";
            if (!s.StartsWith(startKey, StringComparison.OrdinalIgnoreCase) || !s.EndsWith(endKey))
            {
                continue;
            }

            var inner = s[startKey.Length..^endKey.Length];
            var children = Split(inner).Select(ParseNode).ToList();
            return new CompositeFilterNode(logicalOperator, children);
        }

        return ParseLeaf(s);
    }

    private static FieldFilterNode ParseLeaf(string s)
    {
        var firstColon = s.IndexOf(ComponentSeparator);
        if (firstColon < 0)
        {
            throw new FormatException(
                $"Invalid filter expression '{s}'. Expected format: field:op[:value]."
            );
        }

        var field = s[..firstColon];
        var rest = s[(firstColon + 1)..];

        var secondColon = rest.IndexOf(ComponentSeparator);
        string opStr;
        string? value;

        if (secondColon < 0)
        {
            opStr = rest;
            value = null;
        }
        else
        {
            opStr = rest[..secondColon];
            value = rest[(secondColon + 1)..];
        }

        var op = ParseOperator(opStr);

        if (op is FilterOperator.Null or FilterOperator.NotNull)
        {
            value = null;
        }
        else if (value is null)
        {
            throw new FormatException(
                $"Operator '{opStr}' requires a value in filter expression '{s}'."
            );
        }

        return new FieldFilterNode(field, op, value);
    }

    private static FilterOperator ParseOperator(string op)
    {
        return Enum.TryParse(op, true, out FilterOperator result)
            ? result
            : throw new FormatException($"Invalid filter operator '{op}'.");
    }

    private static List<string> Split(string s)
    {
        var parts = new List<string>();
        var depth = 0;
        var start = 0;

        for (var i = 0; i < s.Length; i++)
        {
            switch (s[i])
            {
                case GroupStart:
                    depth++;
                    break;
                case GroupEnd:
                    depth--;
                    break;
                case GroupSeparator when depth == 0:
                    parts.Add(s[start..i]);
                    start = i + 1;
                    break;
            }
        }

        parts.Add(s[start..]);
        return parts;
    }
}
