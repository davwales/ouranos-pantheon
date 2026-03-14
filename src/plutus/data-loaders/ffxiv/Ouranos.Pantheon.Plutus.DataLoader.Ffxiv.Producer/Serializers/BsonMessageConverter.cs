using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Ouranos.Pantheon.Core.WebSockets.Serializers;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Serializers;

public sealed class BsonMessageConverter : IMessageConverter
{
    public BsonMessageConverter()
    {
        var objectSerializer = new ObjectSerializer(x => true);
        BsonSerializer.TryRegisterSerializer(objectSerializer);

        ConventionRegistry.Register(
            "BsonMessageConverterConventions",
            new ConventionPack
            {
                new CamelCaseElementNameConvention()
            },
            t => true
        );
    }

    public byte[] Serialize(object data)
    {
        return data.ToBson();
    }

    public object Deserialize(byte[] data, Type targetType)
    {
        return BsonSerializer.Deserialize(data, targetType);
    }
}