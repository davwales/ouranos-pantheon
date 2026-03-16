using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.Serializers;

public sealed class MessageSerializerTests
{
    private readonly IMessageConverter _converter;
    private readonly MessageSerializer _serializer;
    private readonly ITypeResolver _typeResolver;

    public MessageSerializerTests()
    {
        _typeResolver = Substitute.For<ITypeResolver>();
        _converter = Substitute.For<IMessageConverter>();
        _serializer = new MessageSerializer(_typeResolver, _converter);
    }

    [Fact]
    public void Serialize_WhenNotNull_ShouldReturnExpectedValue()
    {
        // Arrange
        var fixture = new Fixture();
        var entity = fixture.Create<TestEntity>();
        var expectedBytes = fixture.CreateMany<byte>().ToArray();

        _converter.Serialize(entity).Returns(expectedBytes);

        // Act
        var actualBytes = _serializer.Serialize(entity);

        // Assert
        actualBytes.ShouldBe(expectedBytes);
    }

    [Fact]
    public void Serialize_WhenNull_ShouldReturnEmptyArray()
    {
        // Act
        var actualBytes = _serializer.Serialize<TestEntity>(null!);

        // Assert
        actualBytes.ShouldBe([]);
    }

    [Fact]
    public void Deserialize_WhenNotAssignable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.CreateMany<byte>().ToArray();

        _typeResolver.ResolveType(bytes).Returns(typeof(string));

        // Act
        var deserialize = () => _serializer.Deserialize<TestEntity>(bytes);

        // Assert
        var actualException = deserialize.ShouldThrow<InvalidOperationException>();

        actualException.Message.ShouldBe(
            $"Resolved type '{typeof(string)}' is not assignable to '{typeof(TestEntity)}'."
        );
    }

    [Fact]
    public void Deserialize_WhenSingleObject_ShouldReturnExpectedValue()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.CreateMany<byte>().ToArray();
        var expectedEntity = fixture.Create<TestEntity>();

        _typeResolver.ResolveType(bytes).Returns(typeof(TestEntity));
        _converter.Deserialize(bytes, typeof(TestEntity)).Returns(expectedEntity);

        // Act
        var actualEntity = _serializer.Deserialize<TestEntity>(bytes);

        // Assert
        actualEntity.ShouldBe(expectedEntity);
    }

    [Fact]
    public void Deserialize_WhenMultipleObjects_ShouldReturnExpectedValues()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.CreateMany<byte>().ToArray();
        var expectedEntities = fixture.CreateMany<TestEntity>().ToArray() as IList<object>;

        _typeResolver.ResolveType(bytes).Returns(typeof(IList<object>));
        _converter.Deserialize(bytes, typeof(IList<object>)).Returns(expectedEntities);

        foreach (var entity in expectedEntities)
        {
            var entityBytes = fixture.CreateMany<byte>().ToArray();
            _typeResolver.ResolveType(entityBytes).Returns(typeof(TestEntity));
            _converter.Serialize(entity).Returns(entityBytes);
            _converter.Deserialize(entityBytes, typeof(TestEntity)).Returns(entity);
        }

        // Act
        var actualEntities = _serializer.Deserialize<IList<object>>(bytes);

        // Assert
        actualEntities.ShouldBe(expectedEntities);
    }
}