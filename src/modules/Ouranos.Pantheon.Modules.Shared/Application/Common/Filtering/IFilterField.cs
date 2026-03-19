using System.Linq.Expressions;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

internal interface IFilterField
{
    Expression BuildBody(FilterOperator op, string? value, ParameterExpression outerParam);
}
