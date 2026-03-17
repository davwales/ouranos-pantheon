namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

public sealed record OsrsWikiOptions(
    string BaseAddress,
    string UserAgent
)
{
    public OsrsWikiOptions() : this(
        BaseAddress: string.Empty,
        UserAgent: string.Empty
    )
    {
    }
}