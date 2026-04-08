using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.WebSockets;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets;

public sealed class WebSocketsModuleTests
{
    [Fact]
    public void AddWebSockets_ShouldRegisterWebSocketWorkerAsHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{WebSocketOptions.SectionName}:Host"] = "wss://test.example.com",
            })
            .Build();

        // Act
        services.AddWebSockets(configuration, _ => { });

        // Assert
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
            sd.ImplementationType == typeof(WebSocketWorker)
        );
    }
}
