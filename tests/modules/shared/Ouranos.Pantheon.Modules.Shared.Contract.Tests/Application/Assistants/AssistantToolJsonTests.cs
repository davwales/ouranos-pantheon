using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Application.Assistants;

public sealed class AssistantToolJsonTests
{
    public enum TestKind
    {
        First,
        Second,
    }

    private sealed record TestData(
        Id<TestEntity> Id,
        TestKind Kind,
        decimal Price,
        string? Missing,
        IReadOnlyList<decimal> Values
    );

    [Fact]
    public void Serialize_WhenGivenData_ShouldWriteCompactCamelCaseJson()
    {
        // Arrange
        AssistantData<TestData> data = new TestData(
            new Id<TestEntity>("abc"),
            TestKind.Second,
            10266666.6667m,
            null,
            [0.0145678m, 2.500m]
        );

        // Act
        var json = AssistantToolJson.Serialize(data);

        // Assert
        json.ShouldBe("""{"id":"abc","kind":"Second","price":10270000,"values":[0.01457,2.5]}""");
    }

    [Fact]
    public void Serialize_WhenGivenANote_ShouldWriteOnlyTheNote()
    {
        // Arrange
        var data = AssistantData<TestData>.WithNote("Nothing here.");

        // Act
        var json = AssistantToolJson.Serialize(data);

        // Assert
        json.ShouldBe("""{"note":"Nothing here."}""");
    }
}
