namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public sealed record XivApiOptions(
    string BaseAddress,
    int ItemCacheMinutes = 1440
)
{
    public XivApiOptions() : this(string.Empty)
    {
    }
}
