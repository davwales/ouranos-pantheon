using System.Linq.Expressions;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;

internal sealed class TypedFilterField<T, TValue>(
    Expression<Func<T, TValue>> selector,
    bool caseInsensitive = false
) : IFilterField
{
    public bool CaseInsensitive => caseInsensitive;

    public Expression BuildBody(FilterOperator op, string? value, ParameterExpression outerParam)
    {
        var propAccess = ParameterSubstitutor.Substitute(
            selector.Body,
            selector.Parameters[0],
            outerParam
        );

        return FilterExpressionBuilder.BuildPredicateBody(
            op,
            value,
            propAccess,
            typeof(TValue),
            caseInsensitive
        );
    }
}
