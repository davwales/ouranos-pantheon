using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Mongo.Serializers;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Infra.Mongo.Tests.Serializers;

public sealed class IdSerializerTests
{
    private readonly IdSerializer<TestEntity> _serializer = new();

    [Fact]
    public void Serialize_WhenGivenValidObjectId_ShouldWriteExpectedValue()
    {
        // Arrange
        var expectedObjectId = ObjectId.GenerateNewId();
        var id = new Id<TestEntity>(expectedObjectId.ToString());
        var writer = Substitute.For<IBsonWriter>();
        var context = BsonSerializationContext.CreateRoot(writer);
        var args = new BsonSerializationArgs();

        // Act
        _serializer.Serialize(context, args, id);

        // Assert
        writer.Received(1).WriteObjectId(expectedObjectId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Serialize_WhenGivenInvalidObjectId_ShouldWriteEmptyObjectId(string? idValue)
    {
        // Arrange
        var id = new Id<TestEntity>(idValue!);
        var writer = Substitute.For<IBsonWriter>();
        var context = BsonSerializationContext.CreateRoot(writer);
        var args = new BsonSerializationArgs();

        // Act
        _serializer.Serialize(context, args, id);

        // Assert
        writer.Received(1).WriteObjectId(ObjectId.Empty);
    }

    [Fact]
    public void Deserialize_WhenGivenValidObjectId_ShouldReadExpectedValue()
    {
        // Arrange
        var objectId = ObjectId.GenerateNewId();
        var reader = Substitute.For<IBsonReader>();
        var context = BsonDeserializationContext.CreateRoot(reader);
        var args = new BsonDeserializationArgs();

        reader.ReadObjectId().Returns(objectId);

        // Act
        var actualId = _serializer.Deserialize(context, args);

        // Assert
        actualId.ShouldBe(new Id<TestEntity>(objectId.ToString()));
    }
}