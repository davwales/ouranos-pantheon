using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Serializers;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv;

public sealed class BsonMessageConverterTests
{
    private readonly BsonMessageConverter _converter = new();

    private sealed record TestPayload(string Name, int Value);

    [Fact]
    public void Serialize_WhenCalled_ShouldReturnNonEmptyBytes()
    {
        // Arrange
        var payload = new TestPayload("hello", 42);

        // Act
        var bytes = _converter.Serialize(payload);

        // Assert
        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Deserialize_WhenGivenSerializedObject_ShouldRoundTrip()
    {
        // Arrange
        var payload = new TestPayload("world", 99);
        var bytes = _converter.Serialize(payload);

        // Act
        var deserialized = _converter.Deserialize(bytes, typeof(TestPayload));

        // Assert
        deserialized.ShouldNotBeNull();
        var result = deserialized.ShouldBeOfType<TestPayload>();
        result.Name.ShouldBe("world");
        result.Value.ShouldBe(99);
    }
}
