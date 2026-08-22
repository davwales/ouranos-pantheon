using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Domain.Common;

public sealed class BaseEntityTests
{
    [Fact]
    public void Constructor_ShouldSetExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedId = fixture.Create<Id<TestEntity>>();

        // Act
        var entity = new TestEntity(expectedId);

        // Assert
        entity.Id.ShouldBe(expectedId);
        entity.CreatedAt.ShouldBe(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entity.UpdatedAt.ShouldBe(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_ShouldModifyUpdatedAt()
    {
        // Arrange
        var fixture = new Fixture();
        var id = fixture.Create<Id<TestEntity>>();
        var entity = new TestEntity(id);
        var initialUpdatedAt = entity.UpdatedAt;

        // Act
        entity.UpdateEntity();

        // Assert
        entity.UpdatedAt.ShouldNotBe(initialUpdatedAt);
    }
}
