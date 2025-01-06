using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Interfaces.Common;

public interface ICreateDatabaseId<T>
{
    Id<T> CreateId();
}