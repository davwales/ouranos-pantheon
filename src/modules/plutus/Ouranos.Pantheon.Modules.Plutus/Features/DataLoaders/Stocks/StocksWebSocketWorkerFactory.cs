using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers.Converters;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;

/// <summary>Builds the kernel <see cref="WebSocketWorker"/> for the Alpaca JSON feed.</summary>
internal static class StocksWebSocketWorkerFactory
{
    public static WebSocketWorker Create(IServiceProvider sp)
    {
        var wsOptions = sp.GetRequiredService<IOptions<StocksDataLoaderOptions>>().Value.WebSocket;
        var healthState = sp.GetService<WebSocketHealthState>();

        var typeMap = new Dictionary<string, Type>
        {
            [ErrorMessage.TypeIndicator] = typeof(ErrorMessage),
            [SuccessMessage.TypeIndicator] = typeof(SuccessMessage),
            [SubscriptionAckMessage.TypeIndicator] = typeof(SubscriptionAckMessage),
            [AlpacaTradeMessage.TypeIndicator] = typeof(AlpacaTradeMessage),
        };

        var converter = new JsonMessageConverter();
        var typeResolver = new JsonTypeResolver("T", typeMap);
        var serializer = new MessageSerializer(typeResolver, converter);
        var registry = new ListenerRegistry(
            serializer,
            sp.GetRequiredService<WebSocketTelemetry>()
        );

        registry.RegisterListener(sp.GetRequiredService<StocksErrorListener>());
        registry.RegisterListener(sp.GetRequiredService<StocksSuccessListener>());
        registry.RegisterListener(sp.GetRequiredService<StocksSubscriptionListener>());
        registry.RegisterListener(sp.GetRequiredService<StocksTradeListener>());

        var client = new WebSocketClient(
            sp.GetRequiredService<ILogger<WebSocketClient>>(),
            wsOptions.Host,
            wsOptions.BufferSize,
            serializer,
            [],
            registry,
            sp.GetRequiredService<WebSocketTelemetry>()
        );

        return new WebSocketWorker(
            sp.GetRequiredService<ILogger<WebSocketWorker>>(),
            client,
            Options.Create(wsOptions),
            "stocks",
            healthState
        );
    }
}
