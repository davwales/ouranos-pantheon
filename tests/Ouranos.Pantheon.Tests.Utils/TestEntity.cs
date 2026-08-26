using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Tests.Utils;

public sealed class TestEntity(Id<TestEntity> id) : BaseEntity<Id<TestEntity>>(id)
{
    public void UpdateEntity()
    {
        Update();
    }
}
