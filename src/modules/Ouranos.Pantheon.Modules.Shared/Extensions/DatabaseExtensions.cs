using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Modules.Shared.Extensions;

public static class DatabaseExtensions
{
    public static Id<T> CreateId<T>() where T : BaseEntity<Id<T>> => new(Guid.NewGuid().ToString());
}