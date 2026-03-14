namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Infra.XivApi;

public sealed record XivApiOptions(
    string BaseAddress,
    int ItemCacheMinutes = 1440
)
{
    public const string SectionName = "Ouranos:XivApi";

    public XivApiOptions() : this(string.Empty)
    {
    }
}