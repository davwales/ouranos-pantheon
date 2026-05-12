using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;
using Ouranos.Pantheon.Modules.Shared.WebSockets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;

public sealed record FfxivDataLoaderOptions(
    bool IsEnabled,
    WebSocketOptions WebSocket,
    XivApiOptions XivApi,
    IReadOnlyCollection<int> Worlds
)
{
    public const string SectionName = "Ffxiv";

    public FfxivDataLoaderOptions()
        : this(
            IsEnabled: true,
            WebSocket: new WebSocketOptions(),
            XivApi: new XivApiOptions(),
            Worlds: []
        ) { }
}
