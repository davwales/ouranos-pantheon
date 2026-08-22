using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Domain.Common;

public sealed class IdTryParseTests
{
    [Fact]
    public void TryParse_WhenValidString_ShouldReturnTrue()
    {
        // Arrange
        var value = Guid.NewGuid().ToString();

        // Act
        var success = Id<TestEntity>.TryParse(value, null, out var result);

        // Assert
        success.ShouldBeTrue();
        result.Value.ShouldBe(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParse_WhenNullOrWhitespace_ShouldReturnFalse(string? input)
    {
        // Act
        var success = Id<TestEntity>.TryParse(input, null, out var result);

        // Assert
        success.ShouldBeFalse();
        result.ShouldBe(default);
    }
}
