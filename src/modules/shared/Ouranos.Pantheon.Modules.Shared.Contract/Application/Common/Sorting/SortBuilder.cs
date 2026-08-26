using System.Linq.Expressions;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

public sealed class SortBuilder<T>
{
    private readonly Dictionary<string, Func<IQueryable<T>, SortDirection, IQueryable<T>>> _map =
        new(StringComparer.OrdinalIgnoreCase);

    private Func<IQueryable<T>, IQueryable<T>>? _default;

    public SortBuilder<T> On<TKey>(string field, Expression<Func<T, TKey>> keySelector)
    {
        _map[field] = (q, sortDirection) => Sort(q, keySelector, sortDirection);
        return this;
    }

    public SortBuilder<T> Default<TKey>(
        Expression<Func<T, TKey>> keySelector,
        SortDirection sortDirection = SortDirection.Desc
    )
    {
        _default = q => Sort(q, keySelector, sortDirection);
        return this;
    }

    internal IQueryable<T> Apply(IQueryable<T> query, string? field, string? direction)
    {
        if (field is null || !_map.TryGetValue(field, out var apply))
        {
            return _default?.Invoke(query) ?? query;
        }

        if (!Enum.TryParse<SortDirection>(direction, true, out var sortDirection))
        {
            throw new ArgumentException($"Invalid sort direction: {direction}", nameof(direction));
        }

        return apply(query, sortDirection);
    }

    private static IQueryable<T> Sort<TKey>(
        IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        SortDirection sortDirection = SortDirection.Desc
    )
    {
        return sortDirection switch
        {
            SortDirection.Asc => query.OrderBy(keySelector),
            SortDirection.Desc => query.OrderByDescending(keySelector),
            _ => throw new ArgumentException("Invalid sort direction", nameof(sortDirection)),
        };
    }
}
