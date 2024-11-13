using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Application.Interfaces.Common;

public interface ICreateDatabaseId<T>
{
    Id<T> CreateId();
}