using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Domain.Common;

public sealed class IdTests
{
    [Fact]
    public void StringCast_WhenGivenId_ShouldReturnExpectedString()
    {
        // Arrange
        var fixture = new Fixture();
        var id = fixture.Create<Id<TestEntity>>();

        // Act
        var actualStringId = (string)id;

        // Assert
        actualStringId.ShouldBe(id.Value);
    }

    [Fact]
    public void IdCast_WhenGivenString_ShouldReturnExpectedId()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedId = fixture.Create<Id<TestEntity>>();

        // Act
        var actualId = (Id<TestEntity>)expectedId.Value;

        // Assert
        actualId.ShouldBe(expectedId);
    }

    [Fact]
    public void ToString_WhenGivenId_ShouldReturnExpectedString()
    {
        // Arrange
        var fixture = new Fixture();
        var id = fixture.Create<Id<TestEntity>>();

        // Act
        var actualString = id.ToString();

        // Assert
        actualString.ShouldBe(id.Value);
    }

    [Fact]
    public void Parse_ShouldReturnExpectedId()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedId = fixture.Create<Id<TestEntity>>();

        // Act
        var actualId = Id<TestEntity>.Parse(expectedId.Value, null);

        // Assert
        actualId.ShouldBe(expectedId);
    }
}