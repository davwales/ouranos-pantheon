namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

public static class SortExtensions
{
    public static IQueryable<T> SortBy<T>(
        this IQueryable<T> query,
        string? sortField,
        string? sortDirection,
        SortBuilder<T> builder
    ) => builder.Apply(query, sortField, sortDirection);

    public static IQueryable<T> SortBy<T>(
        this IQueryable<T> query,
        string? sortField,
        string? sortDirection,
        Action<SortBuilder<T>> configure
    )
    {
        var builder = new SortBuilder<T>();
        configure(builder);
        return builder.Apply(query, sortField, sortDirection);
    }
}
