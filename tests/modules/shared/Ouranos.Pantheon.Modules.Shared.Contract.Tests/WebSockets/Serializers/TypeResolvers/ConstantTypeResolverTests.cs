using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.Serializers.TypeResolvers;

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
