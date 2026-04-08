using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Models;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv;

public sealed class FfxivMessagesTests
{
    [Fact]
    public void SaleDetail_AllProperties_ShouldBeAccessible()
    {
        // Arrange & Act
        var sale = new SaleDetail(
            Hq: true,
            PricePerUnit: 500,
            Quantity: 3,
            Timestamp: 1700000000,
            OnMannequin: false,
            WorldName: "Gilgamesh",
            WorldId: 34,
            BuyerName: "TestBuyer",
            Total: 1500
        );

        // Assert
        sale.Hq.ShouldBeTrue();
        sale.PricePerUnit.ShouldBe(500);
        sale.Quantity.ShouldBe(3);
        sale.Timestamp.ShouldBe(1700000000);
        sale.OnMannequin.ShouldBeFalse();
        sale.WorldName.ShouldBe("Gilgamesh");
        sale.WorldId.ShouldBe(34);
        sale.BuyerName.ShouldBe("TestBuyer");
        sale.Total.ShouldBe(1500);
    }

    [Fact]
    public void SaleMessage_AllProperties_ShouldBeAccessible()
    {
        // Arrange
        var sale = new SaleDetail(false, 100, 1, 1700000000, false, null, null, "Buyer", 100);
        var message = new SaleMessage("sales/add", 1234, 34, [sale]);

        // Assert
        message.Event.ShouldBe("sales/add");
        message.Item.ShouldBe(1234);
        message.World.ShouldBe(34);
        message.Sales.Count.ShouldBe(1);
    }

    [Fact]
    public void SubscribeMessage_EventDefault_ShouldBeSubscribe()
    {
        // Arrange & Act
        var message = new SubscribeMessage("sales/add");

        // Assert
        message.Channel.ShouldBe("sales/add");
        message.Event.ShouldBe("subscribe");
    }
}
