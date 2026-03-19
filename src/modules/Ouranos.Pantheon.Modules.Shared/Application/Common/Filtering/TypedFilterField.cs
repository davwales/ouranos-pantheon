using System.Linq.Expressions;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

internal sealed class TypedFilterField<T, TValue>(Expression<Func<T, TValue>> selector) : IFilterField
{
    public Expression BuildBody(FilterOperator op, string? value, ParameterExpression outerParam)
    {
        var propAccess = ParameterSubstitutor.Substitute(
            selector.Body,
            selector.Parameters[0],
            outerParam
        );

        return FilterExpressionBuilder.BuildPredicateBody(op, value, propAccess, typeof(TValue));
    }
}
