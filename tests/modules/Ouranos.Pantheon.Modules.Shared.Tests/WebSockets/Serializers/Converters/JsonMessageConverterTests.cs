using System.Text;
using System.Text.Json;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.Converters;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.WebSockets.Serializers.Converters;

public sealed class JsonMessageConverterTests
{
    private readonly JsonMessageConverter _converter;
    private readonly JsonSerializerOptions _options = new();

    public JsonMessageConverterTests()
    {
        _converter = new JsonMessageConverter(_options);
    }

    [Fact]
    public void Serialize_ShouldReturnExpectedResult()
    {
        // Arrange
        var fixture = new Fixture();
        var message = fixture.Create<TestEntity>();

        // Act
        var actualResult = _converter.Serialize(message);

        // Assert
        actualResult.ShouldBe(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, _options)));
    }

    [Fact]
    public void Deserialize_ShouldReturnExpectedResult()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedEntity = fixture.Create<TestEntity>();
        var bytes = _converter.Serialize(expectedEntity);

        // Act
        var actualResult = _converter.Deserialize(bytes, typeof(TestEntity));

        // Assert
        var actualEntity = actualResult.ShouldBeOfType<TestEntity>();
        actualEntity.Id.ShouldBe(expectedEntity.Id);
    }
}
