using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Extensions;

public static class DatabaseExtensions
{
    public static Id<T> CreateId<T>()
        where T : BaseEntity<Id<T>> => new(Guid.NewGuid().ToString());
}
