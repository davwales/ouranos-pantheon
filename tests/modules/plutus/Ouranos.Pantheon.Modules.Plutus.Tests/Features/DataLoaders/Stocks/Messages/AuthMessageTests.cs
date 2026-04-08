using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Stocks.Messages;

public sealed class AuthMessageTests
{
    [Fact]
    public void AuthMessage_AllProperties_ShouldBeAccessible()
    {
        // Arrange & Act
        var message = new AuthMessage("my-key", "my-secret");

        // Assert
        message.Key.ShouldBe("my-key");
        message.Secret.ShouldBe("my-secret");
        message.Action.ShouldBe("auth");
    }
}
