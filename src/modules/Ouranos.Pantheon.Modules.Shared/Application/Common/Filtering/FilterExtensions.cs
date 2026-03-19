namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

public static class FilterExtensions
{
    /// <summary>
    /// Applies a collection of filter expression strings to an <see cref="IQueryable{T}"/>.
    /// Each string is parsed independently and ANDed together (i.e. chained <c>.Where</c> calls).
    ///
    /// <para>
    /// Filterable fields must be explicitly registered on the <see cref="FilterBuilder{T}"/>.
    /// Use <see cref="FilterBuilder{T}.On{TValue}"/> for zero-reflection field mapping, or
    /// <see cref="FilterBuilder{T}.AutoMap"/> to opt in to automatic reflection-based discovery.
    /// </para>
    ///
    /// <para>
    /// Filter syntax:
    /// <list type="bullet">
    ///   <item><description>Leaf:      <c>field:op:value</c> — e.g. <c>price:gt:100</c>, <c>name:like:sword</c>, <c>deletedAt:null</c></description></item>
    ///   <item><description>And group: <c>and(expr|expr|...)</c></description></item>
    ///   <item><description>Or group:  <c>or(expr|expr|...)</c></description></item>
    /// </list>
    /// Groups may be nested. Bind to an input record via <c>string[]? Filter = null</c>
    /// and pass multiple <c>?filter=</c> query-string parameters to AND them together.
    /// </para>
    ///
    /// <para>
    /// Supported operators: <c>eq</c>, <c>ne</c>, <c>lt</c>, <c>lte</c>, <c>gt</c>, <c>gte</c>,
    /// <c>like</c>, <c>startswith</c>, <c>endswith</c>, <c>in</c> (comma-separated values),
    /// <c>null</c>, <c>notnull</c>.
    /// </para>
    /// </summary>
    /// <example>
    /// Explicit field mapping (no reflection at query time):
    /// <code>
    /// q.ApplyFilters(input.Filter, builder => builder
    ///     .On(nameof(Market.Name), m => m.Name)
    ///     .On(nameof(Market.IsForecastingEnabled), m => m.IsForecastingEnabled));
    /// </code>
    /// Opt-in reflection via AutoMap:
    /// <code>
    /// q.ApplyFilters(input.Filter, builder => builder.AutoMap());
    /// </code>
    /// </example>
    /// <exception cref="FormatException">Thrown when a filter string cannot be parsed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a filter references an unregistered field or uses an incompatible operator.</exception>
    /// <summary>
    /// Applies filters using a pre-configured <see cref="FilterBuilder{T}"/> instance.
    /// The builder is built once (e.g. as a <c>static readonly</c> field) and reused
    /// across requests with no allocation or reflection overhead per call.
    /// </summary>
    public static IQueryable<T> FilterBy<T>(
        this IQueryable<T> query,
        IEnumerable<string>? filters,
        FilterBuilder<T> builder
    )
    {
        if (filters is null)
        {
            return query;
        }

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                continue;
            }

            var node = FilterParser.Parse(filter);
            var expression = builder.Build(node);
            query = query.Where(expression);
        }

        return query;
    }

    /// <summary>
    /// Applies filters using an inline configuration action. A new <see cref="FilterBuilder{T}"/>
    /// is created and configured on every call. Prefer the <see cref="FilterBuilder{T}"/> overload
    /// with a <c>static readonly</c> builder when the field set is fixed.
    /// </summary>
    public static IQueryable<T> FilterBy<T>(
        this IQueryable<T> query,
        IEnumerable<string>? filters,
        Action<FilterBuilder<T>> configure
    )
    {
        if (filters is null)
        {
            return query;
        }

        var builder = new FilterBuilder<T>();
        configure(builder);

        return query.FilterBy(filters, builder);
    }
}
