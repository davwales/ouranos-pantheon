using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;

/// <summary>
/// Internal helper that builds a predicate <see cref="Expression"/> body from a resolved
/// property access expression, an operator, and a raw string value.
/// Called by <see cref="TypedFilterField{T,TValue}"/>; not part of the public API.
/// </summary>
internal static class FilterExpressionBuilder
{
    private static readonly MethodInfo ToLowerMethod =
        typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)
        ?? throw new MissingMethodException(nameof(String), nameof(string.ToLower));

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
        Type propType,
        bool caseInsensitive = false
    )
    {
        var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

        if (op is FilterOperator.Null or FilterOperator.NotNull)
        {
            return BuildNullCheck(
                propAccess,
                propType,
                underlyingType,
                isNull: op == FilterOperator.Null
            );
        }

        if (value is null)
        {
            throw new InvalidOperationException($"Operator '{op}' requires a value.");
        }

        var effectivePropAccess =
            caseInsensitive && underlyingType == typeof(string)
                ? Expression.Call(propAccess, ToLowerMethod)
                : propAccess;

        var effectiveValue =
            caseInsensitive && underlyingType == typeof(string) ? value.ToLower() : value;

        return op switch
        {
            FilterOperator.Like => BuildStringOp(
                effectivePropAccess,
                underlyingType,
                effectiveValue,
                nameof(string.Contains)
            ),
            FilterOperator.StartsWith => BuildStringOp(
                effectivePropAccess,
                underlyingType,
                effectiveValue,
                nameof(string.StartsWith)
            ),
            FilterOperator.EndsWith => BuildStringOp(
                effectivePropAccess,
                underlyingType,
                effectiveValue,
                nameof(string.EndsWith)
            ),
            FilterOperator.In => BuildInExpression(
                effectivePropAccess,
                propType,
                underlyingType,
                effectiveValue
            ),
            _ => BuildComparisonExpression(
                op,
                effectivePropAccess,
                propType,
                underlyingType,
                effectiveValue
            ),
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

        var method =
            typeof(string).GetMethod(methodName, [typeof(string)])
            ?? throw new MissingMethodException(nameof(String), methodName);
        return Expression.Call(propAccess, method, Expression.Constant(value));
    }

    private static MethodCallExpression BuildInExpression(
        Expression propAccess,
        Type propType,
        Type underlyingType,
        string value
    )
    {
        var rawValues = value.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        var convertedValues = rawValues.Select(v => ConvertValue(v, underlyingType)).ToList();

        var listType = typeof(List<>).MakeGenericType(underlyingType);
        var list =
            Activator.CreateInstance(listType) as IList
            ?? throw new InvalidOperationException(
                $"Failed to create List<{underlyingType.Name}>."
            );
        foreach (var v in convertedValues)
        {
            list.Add(v);
        }

        var containsMethod =
            listType.GetMethod(nameof(List<>.Contains), [underlyingType])
            ?? throw new MissingMethodException(listType.Name, nameof(List<>.Contains));

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

        if (op is FilterOperator.Eq or FilterOperator.Neq)
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
            _ => throw new NotSupportedException(
                $"Operator '{op}' is not supported as a comparison."
            ),
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

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value, ignoreCase: true);
        }

        return TypeMap.TryGetValue(targetType, out var convert)
            ? convert(value)
            : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
}
