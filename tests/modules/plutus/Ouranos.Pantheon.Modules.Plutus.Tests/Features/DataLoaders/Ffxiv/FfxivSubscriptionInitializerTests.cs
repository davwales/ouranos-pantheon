using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv;

public sealed class FfxivSubscriptionInitializerTests
{
    private readonly ILogger<FfxivSubscriptionInitializer> _logger = Substitute.For<
        ILogger<FfxivSubscriptionInitializer>
    >();

    private readonly IWebSocketClient _client = Substitute.For<IWebSocketClient>();

    private FfxivSubscriptionInitializer CreateInitializer(IReadOnlyCollection<int> worlds)
    {
        return new(
            _logger,
            Options.Create(
                new FfxivDataLoaderOptions(
                    IsEnabled: true,
                    WebSocket: new WebSocketOptions(),
                    XivApi: new XivApiOptions(),
                    Worlds: worlds
                )
            )
        );
    }

    [Fact]
    public async Task OnConnectedAsync_WhenNoWorldsConfigured_ShouldSubscribeGlobally()
    {
        // Arrange
        var initializer = CreateInitializer([]);

        // Act
        await initializer.OnConnectedAsync(_client, CancellationToken.None);

        // Assert
        await _client
            .Received(1)
            .SendAsync(
                Arg.Is<SubscribeMessage>(m => m.Channel == "sales/add"),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task OnConnectedAsync_WhenWorldsConfigured_ShouldSubscribePerWorld()
    {
        // Arrange
        var initializer = CreateInitializer([34, 44]);

        // Act
        await initializer.OnConnectedAsync(_client, CancellationToken.None);

        // Assert
        await _client
            .Received(2)
            .SendAsync(
                Arg.Is<SubscribeMessage>(m => m.Channel.StartsWith("sales/add{world=")),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task OnConnectedAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var initializer = CreateInitializer([]);
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await initializer.OnConnectedAsync(_client, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
