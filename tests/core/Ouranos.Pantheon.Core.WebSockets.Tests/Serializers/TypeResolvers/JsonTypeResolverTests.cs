using System.Text;
using System.Text.Json;
using Ouranos.Pantheon.Core.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.WebSockets.Tests.Serializers.TypeResolvers;

public sealed class JsonTypeResolverTests
{
    [Fact]
    public void ResolveType_WhenArray_ShouldReturnListOfObjects()
    {
        // Arrange
        var fixture = new Fixture();
        var messages = fixture.CreateMany<TestMessage>().ToArray();
        var typeMap = fixture.Create<Dictionary<string, Type>>();
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messages));
        var resolver = new JsonTypeResolver(string.Empty, typeMap);

        // Act
        var actualType = resolver.ResolveType(bytes);

        // Assert
        actualType.ShouldBe(typeof(IList<object>));
    }

    [Fact]
    public void ResolveType_WhenDiscriminatorNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var fixture = new Fixture();
        var message = fixture.Create<TestMessage>();
        var typeMap = fixture.Create<Dictionary<string, Type>>();
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var resolver = new JsonTypeResolver("fakeProperty", typeMap);

        // Act
        var resolve = () => resolver.ResolveType(bytes);

        // Assert
        var actualException = resolve.ShouldThrow<InvalidOperationException>();
        actualException.Message.ShouldBe("Discriminator path 'fakeProperty' not found.");
    }

    [Fact]
    public void ResolveType_WhenTypeMappingNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var fixture = new Fixture();
        var message = fixture.Create<TestMessage>();
        var typeMap = new Dictionary<string, Type>();
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var resolver = new JsonTypeResolver(nameof(TestMessage.Type), typeMap);

        // Act
        var resolve = () => resolver.ResolveType(bytes);

        // Assert
        var actualException = resolve.ShouldThrow<InvalidOperationException>();
        actualException.Message.ShouldBe($"No type mapping found for discriminator: {message.Type}.");
    }

    [Fact]
    public void ResolveType_WhenTypeMappingFound_ShouldReturnExpectedType()
    {
        // Arrange
        var fixture = new Fixture();
        var message = new TestMessage("test", fixture.Create<TestEntity>());
        var expectedType = typeof(TestEntity);
        var typeMap = new Dictionary<string, Type>
        {
            { message.Type, expectedType }
        };

        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var resolver = new JsonTypeResolver(nameof(TestMessage.Type), typeMap);

        // Act
        var actualType = resolver.ResolveType(bytes);

        // Assert
        actualType.ShouldBe(expectedType);
    }

    private sealed record TestMessage(string Type, object Content);
}