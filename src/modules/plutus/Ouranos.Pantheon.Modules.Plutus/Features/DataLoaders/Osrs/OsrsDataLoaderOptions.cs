namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;

public sealed record OsrsDataLoaderOptions(
    bool IsEnabled,
    int RefreshIntervalMinutes,
    OsrsWikiOptions Wiki
)
{
    public const string SectionName = "Osrs";

    public OsrsDataLoaderOptions() : this(
        IsEnabled: true,
        RefreshIntervalMinutes: 5,
        Wiki: new OsrsWikiOptions()
    )
    {
    }
}
