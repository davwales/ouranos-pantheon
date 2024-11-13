using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Infra.Mongo.Serializers;

public class IdSerializer<T> : SerializerBase<Id<T>>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Id<T> id)
    {
        var objectId = string.IsNullOrWhiteSpace(id.Value) ? ObjectId.Empty : ObjectId.Parse(id.Value);
        context.Writer.WriteObjectId(objectId);
    }

    public override Id<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var objectId = context.Reader.ReadObjectId();
        return new Id<T>(objectId.ToString());
    }
}