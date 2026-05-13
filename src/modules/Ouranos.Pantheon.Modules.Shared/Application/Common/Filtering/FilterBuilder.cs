using System.Linq.Expressions;
using System.Reflection;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

/// <summary>
/// Configures which fields on <typeparamref name="T"/> are exposed as filterable,
/// and how each field maps to a property expression.
///
/// <para>
/// Use <see cref="On{TValue}"/> to register fields explicitly (no reflection).
/// Use <see cref="AutoMap"/> to automatically register all public properties via reflection.
/// </para>
/// </summary>
public sealed class FilterBuilder<T>
{
    private readonly Dictionary<string, IFilterField> _fields = new(
        StringComparer.OrdinalIgnoreCase
    );

    /// <summary>
    /// Registers a filterable field by name, backed by the given property selector.
    /// No reflection is used at query time.
    /// </summary>
    public FilterBuilder<T> On<TValue>(
        string key,
        Expression<Func<T, TValue>> selector,
        bool caseInsensitive = false
    )
    {
        _fields[key] = new TypedFilterField<T, TValue>(selector, caseInsensitive);
        return this;
    }

    /// <summary>
    /// Automatically registers all public instance properties of <typeparamref name="T"/>
    /// as filterable fields. Uses reflection once per <see cref="AutoMap"/> call.
    /// </summary>
    public FilterBuilder<T> AutoMap()
    {
        var param = Expression.Parameter(typeof(T), "x");

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propAccess = Expression.Property(param, prop);
            var lambdaType = typeof(Func<,>).MakeGenericType(typeof(T), prop.PropertyType);
            var lambda = Expression.Lambda(lambdaType, propAccess, param);
            var fieldType = typeof(TypedFilterField<,>).MakeGenericType(
                typeof(T),
                prop.PropertyType
            );
            _fields[prop.Name] =
                Activator.CreateInstance(fieldType, lambda, false) as IFilterField
                ?? throw new InvalidOperationException(
                    $"Failed to create filter field for property '{prop.Name}' on '{typeof(T).Name}'."
                );
        }

        return this;
    }

    internal Expression<Func<T, bool>> Build(FilterNode node)
    {
        var param = Expression.Parameter(typeof(T), "x");
        return Expression.Lambda<Func<T, bool>>(BuildBody(node, param), param);
    }

    private Expression BuildBody(FilterNode node, ParameterExpression param)
    {
        return node switch
        {
            FieldFilterNode field => BuildFieldBody(field, param),
            CompositeFilterNode composite => BuildCompositeBody(composite, param),
            _ => throw new NotSupportedException(
                $"Unknown filter node type '{node.GetType().Name}'."
            ),
        };
    }

    private Expression BuildCompositeBody(CompositeFilterNode node, ParameterExpression param)
    {
        if (node.Children.Count == 0)
        {
            throw new InvalidOperationException(
                "Composite filter node must have at least one child."
            );
        }

        var parts = node.Children.Select(c => BuildBody(c, param)).ToList();

        return node.Logic == LogicalOperator.And
            ? parts.Aggregate(Expression.AndAlso)
            : parts.Aggregate(Expression.OrElse);
    }

    private Expression BuildFieldBody(FieldFilterNode node, ParameterExpression param)
    {
        if (_fields.TryGetValue(node.Field, out var field))
        {
            return field.BuildBody(node.Operator, node.Value, param);
        }

        var registered = string.Join(", ", _fields.Keys);
        throw new InvalidOperationException(
            $"Field '{node.Field}' is not registered as filterable on '{typeof(T).Name}'. "
                + $"Registered fields: {registered}."
        );
    }
}
