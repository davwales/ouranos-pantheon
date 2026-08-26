using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets;

public static class WebSocketsModule
{
    public static IServiceCollection AddWebSockets(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IWebSocketClientBuilder> configureWebSocketClient
    )
    {
        var options = configuration
            .GetSection(WebSocketOptions.SectionName)
            .Get<WebSocketOptions?>();
        var builder = new WebSocketClientBuilder(services, options);
        configureWebSocketClient(builder);
        builder.Build();
        return services.AddHostedService<WebSocketWorker>();
    }
}
