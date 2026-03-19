namespace Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;

public static class PaginationExtensions
{
    /// <summary>
    /// Applies skip/take pagination to an <see cref="IQueryable{T}"/>.
    /// </summary>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int skip, int take)
    {
        return query.Skip(skip).Take(take);
    }
}
