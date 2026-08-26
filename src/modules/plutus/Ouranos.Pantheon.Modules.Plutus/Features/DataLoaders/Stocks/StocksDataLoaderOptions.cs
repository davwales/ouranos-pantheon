using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;

public sealed record StocksDataLoaderOptions(
    bool IsEnabled,
    string ApiKey,
    string ApiSecret,
    WebSocketOptions WebSocket,
    IReadOnlyCollection<string> Symbols
)
{
    public const string SectionName = "Stocks";

    public StocksDataLoaderOptions()
        : this(
            IsEnabled: true,
            ApiKey: string.Empty,
            ApiSecret: string.Empty,
            WebSocket: new WebSocketOptions(),
            Symbols: []
        ) { }
}
