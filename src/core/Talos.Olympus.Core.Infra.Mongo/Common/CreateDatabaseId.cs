using MongoDB.Bson;
using Talos.Olympus.Core.Application.Interfaces.Common;
using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Infra.Mongo.Common;

public sealed class CreateDatabaseId<T> : ICreateDatabaseId<T>
{
    public Id<T> CreateId()
    {
        var mongoId = ObjectId.GenerateNewId().ToString();
        return new Id<T>(mongoId);
    }
}