using Ouranos.Pantheon.Core.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.WebSockets.Tests.Serializers.TypeResolvers;

public sealed class ConstantTypeResolverTests
{
    [Fact]
    public void ResolveType_ShouldReturnExpectedType()
    {
        // Arrange
        var fixture = new Fixture();
        var bytes = fixture.CreateMany<byte>().ToArray();
        var expectedType = typeof(TestEntity);
        var resolver = new ConstantTypeResolver(expectedType);

        // Act
        var actualType = resolver.ResolveType(bytes);

        // Assert
        actualType.ShouldBe(expectedType);
    }
}