using System.Linq.Expressions;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;

internal interface IFilterField
{
    bool CaseInsensitive { get; }

    Expression BuildBody(FilterOperator op, string? value, ParameterExpression outerParam);
}
