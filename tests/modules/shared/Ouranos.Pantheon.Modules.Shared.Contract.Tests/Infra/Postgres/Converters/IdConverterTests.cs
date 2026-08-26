using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Infra.Postgres.Converters;

public sealed class IdConverterTests
{
    private sealed class FakeEntity;

    private readonly IdConverter<FakeEntity> _converter = new();

    [Fact]
    public void ConvertToProvider_ShouldConvertIdToGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new Id<FakeEntity>(guid.ToString());

        // Act
        var result = _converter.ConvertToProviderExpression.Compile()(id);

        // Assert
        result.ShouldBe(guid);
    }

    [Fact]
    public void ConvertFromProvider_ShouldConvertGuidToId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = _converter.ConvertFromProviderExpression.Compile()(guid);

        // Assert
        result.Value.ShouldBe(guid.ToString());
    }

    [Fact]
    public void RoundTrip_ShouldPreserveValue()
    {
        // Arrange
        var original = new Id<FakeEntity>(Guid.NewGuid().ToString());

        // Act
        var guid = _converter.ConvertToProviderExpression.Compile()(original);
        var restored = _converter.ConvertFromProviderExpression.Compile()(guid);

        // Assert
        restored.Value.ShouldBe(original.Value);
    }
}
