using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Extensions;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Extensions;

public sealed class IdExtensionsTests
{
    [Fact]
    public void GetStreamId_WhenValueIsValidGuid_ShouldReturnExpectedGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new Id<TestEventSourcedEntity>(guid.ToString());

        // Act
        var streamId = id.GetStreamId();

        // Assert
        streamId.ShouldBe(guid);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("")]
    [InlineData("   ")]
    public void GetStreamId_WhenValueIsInvalid_ShouldThrowFormatException(string value)
    {
        // Arrange
        var id = new Id<TestEventSourcedEntity>(value);

        // Act
        Action act = () => id.GetStreamId();

        // Assert
        act.ShouldThrow<FormatException>();
    }

    [Fact]
    public void GetStreamId_WhenValueIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = default(Id<TestEventSourcedEntity>);

        // Act
        Action act = () => id.GetStreamId();

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void TryGetStreamId_WhenValueIsValidGuid_ShouldReturnTrueAndExpectedGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new Id<TestEventSourcedEntity>(guid.ToString());

        // Act
        var success = id.TryGetStreamId(out var streamId);

        // Assert
        success.ShouldBeTrue();
        streamId.ShouldBe(guid);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("")]
    [InlineData("   ")]
    public void TryGetStreamId_WhenValueIsInvalid_ShouldReturnFalseAndDefaultGuid(string value)
    {
        // Arrange
        var id = new Id<TestEventSourcedEntity>(value);

        // Act
        var success = id.TryGetStreamId(out var streamId);

        // Assert
        success.ShouldBeFalse();
        streamId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void TryGetStreamId_WhenValueIsNull_ShouldReturnFalseAndDefaultGuid()
    {
        // Arrange
        var id = default(Id<TestEventSourcedEntity>);

        // Act
        var success = id.TryGetStreamId(out var streamId);

        // Assert
        success.ShouldBeFalse();
        streamId.ShouldBe(Guid.Empty);
    }
}
