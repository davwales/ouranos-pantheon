using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;

/// <summary>
/// Writes filter expressions in the <c>field:op:value</c> syntax that <see cref="FilterParser"/>
/// reads, for callers that build list-query inputs in code.
/// </summary>
public static class FilterExpression
{
    public static string For(string field, FilterOperator filterOperator, string value)
    {
        return $"{field}:{Operator(filterOperator)}:{value}";
    }

    public static string Operator(FilterOperator filterOperator)
    {
        return filterOperator.ToString().ToLowerInvariant();
    }
}
