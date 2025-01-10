namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki;

public sealed record OsrsWikiOptions(
    string BaseAddress,
    string UserAgent,
    int RefreshIntervalMinutes
)
{
    public const string SectionName = "Ouranos:OsrsWiki";

    public OsrsWikiOptions() : this(string.Empty, string.Empty, 5)
    {
    }
}