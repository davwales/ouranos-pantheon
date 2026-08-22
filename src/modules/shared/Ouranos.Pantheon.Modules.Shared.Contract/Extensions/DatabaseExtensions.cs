using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

public static class DatabaseExtensions
{
    public static Id<T> CreateId<T>()
        where T : BaseEntity<Id<T>>
    {
        return new(Guid.NewGuid().ToString());
    }
}
