using MongoDB.Bson;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Infra.Mongo.Common;

public sealed class CreateDatabaseId<T> : ICreateDatabaseId<T>
{
    public Id<T> CreateId()
    {
        var mongoId = ObjectId.GenerateNewId().ToString();
        return new Id<T>(mongoId);
    }
}