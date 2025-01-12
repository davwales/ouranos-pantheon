using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets;

public static class WebSocketsModule
{
    public static IServiceCollection AddWebSockets(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IWebSocketClientBuilder> configureWebSocketClient
    )
    {
        var options = configuration.GetSection(WebSocketOptions.SectionName).Get<WebSocketOptions?>();
        var builder = new WebSocketClientBuilder(services, options);
        configureWebSocketClient(builder);
        builder.Build();
        return services.AddHostedService<WebSocketWorker>();
    }
}