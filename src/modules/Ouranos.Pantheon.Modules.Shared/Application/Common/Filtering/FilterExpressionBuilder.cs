using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering.Schemas;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

/// <summary>
/// Internal helper that builds a predicate <see cref="Expression"/> body from a resolved
/// property access expression, an operator, and a raw string value.
/// Called by <see cref="TypedFilterField{T,TValue}"/>; not part of the public API.
/// </summary>
internal static class FilterExpressionBuilder
{
    internal static Dictionary<Type, Func<string, object>> TypeMap = new()
    {
        [typeof(string)] = s => s,
        [typeof(bool)] = s => bool.Parse(s),
        [typeof(int)] = s => int.Parse(s, CultureInfo.InvariantCulture),
        [typeof(long)] = s => long.Parse(s, CultureInfo.InvariantCulture),
        [typeof(float)] = s => float.Parse(s, CultureInfo.InvariantCulture),
        [typeof(double)] = s => double.Parse(s, CultureInfo.InvariantCulture),
        [typeof(decimal)] = s => decimal.Parse(s, CultureInfo.InvariantCulture),
        [typeof(Guid)] = s => Guid.Parse(s),
        [typeof(DateTime)] = s => DateTime.Parse(s, CultureInfo.InvariantCulture),
        [typeof(DateTimeOffset)] = s => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture),
    };

    internal static Expression BuildPredicateBody(
        FilterOperator op,
        string? value,
        Expression propAccess,
        Type propType
    )
    {
        var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

        return op switch
        {
            FilterOperator.Null => BuildNullCheck(propAccess, propType, underlyingType, isNull: true),
            FilterOperator.NotNull => BuildNullCheck(propAccess, propType, underlyingType, isNull: false),
            FilterOperator.Like => BuildStringOp(propAccess, underlyingType, value!, nameof(string.Contains)),
            FilterOperator.StartsWith => BuildStringOp(
                propAccess,
                underlyingType,
                value!,
                nameof(string.StartsWith)
            ),
            FilterOperator.EndsWith => BuildStringOp(
                propAccess,
                underlyingType,
                value!,
                nameof(string.EndsWith)
            ),
            FilterOperator.In => BuildInExpression(propAccess, propType, underlyingType, value!),
            _ => BuildComparisonExpression(op, propAccess, propType, underlyingType, value!),
        };
    }

    private static BinaryExpression BuildNullCheck(
        Expression propAccess,
        Type propType,
        Type underlyingType,
        bool isNull
    )
    {
        var isNullable = propType != underlyingType || !propType.IsValueType;
        if (!isNullable)
        {
            throw new InvalidOperationException(
                $"Cannot apply null/notnull filter to non-nullable property of type '{propType.Name}'."
            );
        }

        var nullConst = Expression.Constant(null, propType);
        return isNull
            ? Expression.Equal(propAccess, nullConst)
            : Expression.NotEqual(propAccess, nullConst);
    }

    private static MethodCallExpression BuildStringOp(
        Expression propAccess,
        Type underlyingType,
        string value,
        string methodName
    )
    {
        if (underlyingType != typeof(string))
        {
            throw new InvalidOperationException(
                $"Operator '{methodName.ToLowerInvariant()}' can only be applied to string properties, "
                + $"but the property type is '{underlyingType.Name}'."
            );
        }

        var method = typeof(string).GetMethod(methodName, [typeof(string)])!;
        return Expression.Call(propAccess, method, Expression.Constant(value));
    }

    private static MethodCallExpression BuildInExpression(
        Expression propAccess,
        Type propType,
        Type underlyingType,
        string value
    )
    {
        var rawValues = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var convertedValues = rawValues.Select(v => ConvertValue(v, underlyingType)).ToList();

        var listType = typeof(List<>).MakeGenericType(underlyingType);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var v in convertedValues)
        {
            list.Add(v);
        }

        var containsMethod = listType.GetMethod(nameof(List<object>.Contains), [underlyingType])!;

        Expression target =
            Nullable.GetUnderlyingType(propType) != null
                ? Expression.Convert(propAccess, underlyingType)
                : propAccess;

        return Expression.Call(Expression.Constant(list), containsMethod, target);
    }

    private static BinaryExpression BuildComparisonExpression(
        FilterOperator op,
        Expression propAccess,
        Type propType,
        Type underlyingType,
        string value
    )
    {
        var converted = ConvertValue(value, underlyingType);
        var isNullableValueType = Nullable.GetUnderlyingType(propType) != null;

        if (op is FilterOperator.Eq or FilterOperator.Ne)
        {
            Expression constant = isNullableValueType
                ? Expression.Convert(Expression.Constant(converted, underlyingType), propType)
                : Expression.Constant(converted, underlyingType);

            return op == FilterOperator.Eq
                ? Expression.Equal(propAccess, constant)
                : Expression.NotEqual(propAccess, constant);
        }

        var left = isNullableValueType
            ? Expression.Convert(propAccess, underlyingType)
            : propAccess;
        var scalar = Expression.Constant(converted, underlyingType);

        return op switch
        {
            FilterOperator.Lt => Expression.LessThan(left, scalar),
            FilterOperator.Lte => Expression.LessThanOrEqual(left, scalar),
            FilterOperator.Gt => Expression.GreaterThan(left, scalar),
            FilterOperator.Gte => Expression.GreaterThanOrEqual(left, scalar),
            _ => throw new NotSupportedException($"Operator '{op}' is not supported as a comparison."),
        };
    }

    internal static object ConvertValue(string value, Type targetType)
    {
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Id<>))
        {
            var ctor = targetType.GetConstructor([typeof(string)]);
            if (ctor is not null)
            {
                return ctor.Invoke([value]);
            }
        }

        return TypeMap.TryGetValue(targetType, out var convert)
            ? convert(value)
            : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
}
