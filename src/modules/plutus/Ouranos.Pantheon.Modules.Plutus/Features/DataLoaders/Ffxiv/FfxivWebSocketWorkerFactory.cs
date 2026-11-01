using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Serializers;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;

/// <summary>Builds the kernel <see cref="WebSocketWorker"/> for the Universalis BSON feed.</summary>
internal static class FfxivWebSocketWorkerFactory
{
    public static WebSocketWorker Create(IServiceProvider sp)
    {
        var wsOptions = sp.GetRequiredService<IOptions<FfxivDataLoaderOptions>>().Value.WebSocket;
        var healthState = sp.GetService<WebSocketHealthState>();

        var converter = new BsonMessageConverter();
        var typeResolver = new ConstantTypeResolver(typeof(SaleMessage));
        var serializer = new MessageSerializer(typeResolver, converter);
        var registry = new ListenerRegistry(
            serializer,
            sp.GetRequiredService<WebSocketTelemetry>()
        );

        var listener = sp.GetRequiredService<FfxivListener>();
        registry.RegisterListener(listener);

        var initializer = sp.GetRequiredService<FfxivSubscriptionInitializer>();

        var client = new WebSocketClient(
            sp.GetRequiredService<ILogger<WebSocketClient>>(),
            wsOptions.Host,
            wsOptions.BufferSize,
            serializer,
            [initializer],
            registry,
            sp.GetRequiredService<WebSocketTelemetry>()
        );

        return new WebSocketWorker(
            sp.GetRequiredService<ILogger<WebSocketWorker>>(),
            client,
            Options.Create(wsOptions),
            "ffxiv",
            healthState
        );
    }
}
