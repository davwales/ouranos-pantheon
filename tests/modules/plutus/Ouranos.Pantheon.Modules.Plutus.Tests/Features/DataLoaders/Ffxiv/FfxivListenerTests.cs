using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Models;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv;

public sealed class FfxivListenerTests
{
    private readonly ILogger<FfxivListener> _logger = Substitute.For<ILogger<FfxivListener>>();
    private readonly IGetItems _getItems = Substitute.For<IGetItems>();
    private readonly IQueueTradeMessages _queue = Substitute.For<IQueueTradeMessages>();
    private readonly IWebSocketClient _client = Substitute.For<IWebSocketClient>();
    private readonly FfxivListener _listener;

    public FfxivListenerTests()
    {
        _listener = new FfxivListener(_logger, _getItems, _queue);
    }

    [Fact]
    public async Task HandleMessageAsync_WhenSalesExistAndItemFound_ShouldQueueTradeMessages()
    {
        // Arrange
        var itemCode = 1234;
        var sale = new SaleDetail(
            Hq: false,
            PricePerUnit: 100,
            Quantity: 5,
            Timestamp: (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            OnMannequin: false,
            WorldName: null,
            WorldId: null,
            BuyerName: "Buyer",
            Total: 500
        );

        var message = new SaleMessage("sales/add", itemCode, 34, [sale]);

        _getItems
            .GetItemAsync(itemCode.ToString(), false, Arg.Any<CancellationToken>())
            .Returns(new ItemDto(itemCode.ToString(), false, "Some Item", new AdditionalFields()));
        _getItems
            .GetItemAsync(itemCode.ToString(), true, Arg.Any<CancellationToken>())
            .Returns(
                new ItemDto(itemCode.ToString(), true, "Some Item HQ", new AdditionalFields())
            );

        // Act
        await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await _queue
            .Received(1)
            .QueueMessages(
                Arg.Is<IReadOnlyCollection<TradeMessage>>(msgs =>
                    msgs.Count == 1
                    && msgs.First().Producer == Producer.Ffxiv
                    && msgs.First().SymbolCode == itemCode.ToString()
                    && msgs.First().SymbolName == "Some Item"
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task HandleMessageAsync_WhenSaleIsHighQuality_ShouldUseHighQualityItem()
    {
        // Arrange
        var itemCode = 1234;
        var sale = new SaleDetail(
            Hq: true,
            PricePerUnit: 100,
            Quantity: 5,
            Timestamp: (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            OnMannequin: false,
            WorldName: null,
            WorldId: null,
            BuyerName: "Buyer",
            Total: 500
        );

        var message = new SaleMessage("sales/add", itemCode, 34, [sale]);

        _getItems
            .GetItemAsync(itemCode.ToString(), false, Arg.Any<CancellationToken>())
            .Returns(new ItemDto(itemCode.ToString(), false, "Some Item", new AdditionalFields()));
        _getItems
            .GetItemAsync(itemCode.ToString(), true, Arg.Any<CancellationToken>())
            .Returns(
                new ItemDto(itemCode.ToString(), true, "Some Item HQ", new AdditionalFields())
            );

        // Act
        await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await _queue
            .Received(1)
            .QueueMessages(
                Arg.Is<IReadOnlyCollection<TradeMessage>>(msgs =>
                    msgs.Count == 1
                    && msgs.First().SymbolSubcode == "hq"
                    && msgs.First().SymbolName == "Some Item HQ"
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task HandleMessageAsync_WhenNoSales_ShouldNotQueueAnyMessages()
    {
        // Arrange
        var message = new SaleMessage("sales/add", 1234, 34, []);

        // Act
        await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await _queue
            .DidNotReceive()
            .QueueMessages(
                Arg.Any<IReadOnlyCollection<TradeMessage>>(),
                Arg.Any<CancellationToken>()
            );
        await _getItems
            .DidNotReceive()
            .GetItemAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleMessageAsync_WhenItemNotFound_ShouldSkipTrade()
    {
        // Arrange
        var itemCode = 9999;
        var sale = new SaleDetail(
            false,
            100,
            5,
            (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            false,
            null,
            null,
            "Buyer",
            500
        );
        var message = new SaleMessage("sales/add", itemCode, 34, [sale]);

        _getItems
            .GetItemAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((ItemDto?)null);

        // Act
        await _listener.HandleMessageAsync(message, _client, CancellationToken.None);

        // Assert
        await _queue
            .Received(1)
            .QueueMessages(
                Arg.Is<IReadOnlyCollection<TradeMessage>>(msgs => msgs.Count == 0),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task HandleMessageAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var sale = new SaleDetail(
            false,
            100,
            5,
            (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            false,
            null,
            null,
            "Buyer",
            500
        );
        var message = new SaleMessage("sales/add", 1234, 34, [sale]);
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await _listener.HandleMessageAsync(message, _client, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
