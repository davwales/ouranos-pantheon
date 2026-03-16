using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common;

public sealed class WrapperResponseTests
{
    [Fact]
    public void Constructor_ShouldSetExpectedValue()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedValue = fixture.Create<TestEntity>();

        // Act
        var wrapper = new WrapperResponse<TestEntity>(expectedValue);

        // Assert
        wrapper.Value.ShouldBe(expectedValue);
    }
}