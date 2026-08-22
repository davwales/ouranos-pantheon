using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Extensions;

public sealed class DatabaseExtensionsTests
{
    private sealed class TestEntity(Id<TestEntity> id) : BaseEntity<Id<TestEntity>>(id);

    [Fact]
    public void CreateId_ShouldReturnNonDefaultId()
    {
        // Act
        var id = DatabaseExtensions.CreateId<TestEntity>();

        // Assert
        id.ShouldNotBe(default);
        id.Value.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void CreateId_CalledTwice_ShouldReturnDistinctIds()
    {
        // Act
        var id1 = DatabaseExtensions.CreateId<TestEntity>();
        var id2 = DatabaseExtensions.CreateId<TestEntity>();

        // Assert
        id1.ShouldNotBe(id2);
    }
}
