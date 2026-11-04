namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

public static class SortDirectionExtensions
{
    /// <summary>The value list-query inputs take as their sort direction.</summary>
    public static string ToQueryValue(this SortDirection sortDirection)
    {
        return sortDirection.ToString().ToLowerInvariant();
    }
}
