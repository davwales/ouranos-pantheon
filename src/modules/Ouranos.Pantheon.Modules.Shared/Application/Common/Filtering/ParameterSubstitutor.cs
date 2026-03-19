using System.Linq.Expressions;

namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

internal static class ParameterSubstitutor
{
    internal static Expression Substitute(
        Expression body,
        ParameterExpression from,
        ParameterExpression to
    ) => new SubstitutorVisitor(from, to).Visit(body);

    private sealed class SubstitutorVisitor(ParameterExpression from, ParameterExpression to)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == from ? to : base.VisitParameter(node);
    }
}
