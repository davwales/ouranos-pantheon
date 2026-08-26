using System.Text.Json;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Postgres.Converters;

public sealed class IdJsonConverterFactoryTests
{
    private sealed class FakeEntity;

    private readonly IdJsonConverterFactory _factory = new();
    private readonly JsonSerializerOptions _options;

    public IdJsonConverterFactoryTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(_factory);
    }

    [Fact]
    public void CanConvert_WhenIdType_ShouldReturnTrue()
    {
        // Act
        var result = _factory.CanConvert(typeof(Id<FakeEntity>));

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanConvert_WhenNonIdType_ShouldReturnFalse()
    {
        // Act
        var result = _factory.CanConvert(typeof(string));

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Serialize_WhenIdProvided_ShouldSerializeAsString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new Id<FakeEntity>(guid.ToString());

        // Act
        var json = JsonSerializer.Serialize(id, _options);

        // Assert
        json.ShouldBe($"\"{guid}\"");
    }

    [Fact]
    public void Deserialize_WhenStringProvided_ShouldDeserializeToId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var json = $"\"{guid}\"";

        // Act
        var result = JsonSerializer.Deserialize<Id<FakeEntity>>(json, _options);

        // Assert
        result.Value.ShouldBe(guid.ToString());
    }

    [Fact]
    public void RoundTrip_ShouldPreserveValue()
    {
        // Arrange
        var original = new Id<FakeEntity>(Guid.NewGuid().ToString());

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var restored = JsonSerializer.Deserialize<Id<FakeEntity>>(json, _options);

        // Assert
        restored.Value.ShouldBe(original.Value);
    }
}
