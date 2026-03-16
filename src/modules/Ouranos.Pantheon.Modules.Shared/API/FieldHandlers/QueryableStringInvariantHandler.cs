using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;

namespace Ouranos.Pantheon.Modules.Shared.API.FieldHandlers;

public sealed class QueryableStringInvariantHandler(InputParser inputParser)
    : QueryableOperationHandlerBase(inputParser)
{
    private static readonly MethodInfo ToLower = typeof(string).GetMethod(nameof(string.ToLower), [])
                                                 ?? throw new InvalidOperationException(
                                                     "Unable to find method to convert strings to lowercase");

    private static readonly Dictionary<int, MethodInfo?> Operators = new()
    {
        {
            DefaultFilterOperations.Equals,
            typeof(string).GetMethod(nameof(string.Equals), [typeof(string)])
        },
        {
            DefaultFilterOperations.Contains,
            typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])
        },
        {
            DefaultFilterOperations.StartsWith,
            typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])
        },
        {
            DefaultFilterOperations.EndsWith,
            typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])
        },
        {
            DefaultFilterOperations.NotEquals,
            typeof(string).GetMethod(nameof(string.Equals), [typeof(string)])
        },
        {
            DefaultFilterOperations.NotContains,
            typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])
        },
        {
            DefaultFilterOperations.NotStartsWith,
            typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])
        },
        {
            DefaultFilterOperations.NotEndsWith,
            typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])
        }
    };

    private static readonly List<int> ExclusionOperators =
    [
        DefaultFilterOperations.NotEquals,
        DefaultFilterOperations.NotContains,
        DefaultFilterOperations.NotStartsWith,
        DefaultFilterOperations.NotEndsWith
    ];

    public override bool CanHandle(
        ITypeCompletionContext context,
        IFilterInputTypeDefinition typeDefinition,
        IFilterFieldDefinition fieldDefinition
    )
    {
        return context.Type is StringOperationFilterInputType &&
               fieldDefinition is FilterOperationFieldDefinition operationField &&
               Operators.ContainsKey(operationField.Id);
    }

    public override Expression HandleOperation(
        QueryableFilterContext context,
        IFilterOperationField field,
        IValueNode value,
        object? parsedValue
    )
    {
        var operation = Operators.GetValueOrDefault(field.Id)
                        ?? throw new InvalidOperationException("Unable to find valid operation method.");

        if (parsedValue is not string str)
        {
            throw new InvalidOperationException("Can only perform invariant string operations on strings.");
        }

        var property = context.GetInstance();
        Expression expression = Expression.Call(
            Expression.Call(property, ToLower),
            operation,
            Expression.Constant(str.ToLower())
        );

        if (ExclusionOperators.Contains(field.Id))
        {
            expression = Expression.Not(expression);
        }

        return expression;
    }
}