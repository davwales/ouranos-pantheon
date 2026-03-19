namespace Ouranos.Pantheon.Modules.Shared.Application.Common;

public sealed record PagedResponse<T>(IEnumerable<T> Items, int TotalCount, int Skip, int Take);
