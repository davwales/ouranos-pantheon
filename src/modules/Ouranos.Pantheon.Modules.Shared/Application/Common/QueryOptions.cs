namespace Ouranos.Pantheon.Modules.Shared.Application.Common;

public sealed record QueryOptions(int MinPageSize, int MaxPageSize, int MaxSkip)
{
    public const string SectionName = "Ouranos:Query";

    public QueryOptions()
        : this(MinPageSize: 1, MaxPageSize: 100, MaxSkip: 10000) { }
}
