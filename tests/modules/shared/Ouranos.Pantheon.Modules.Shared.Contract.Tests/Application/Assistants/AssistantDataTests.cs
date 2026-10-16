using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Application.Assistants;

public sealed class AssistantDataTests
{
    private sealed record TestData(string Name);

    [Fact]
    public void ImplicitConversion_WhenGivenData_ShouldCarryTheDataWithoutANote()
    {
        // Arrange
        var value = new TestData("x");

        // Act
        AssistantData<TestData> data = value;

        // Assert
        data.Data.ShouldBe(value);
        data.Note.ShouldBeNull();
    }

    [Fact]
    public void WithNote_WhenGivenANote_ShouldCarryTheNoteWithoutData()
    {
        // Act
        var data = AssistantData<TestData>.WithNote("Nothing here.");

        // Assert
        data.Data.ShouldBeNull();
        data.Note.ShouldBe("Nothing here.");
    }

    [Fact]
    public void WithNote_WhenNoteIsBlank_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => AssistantData<TestData>.WithNote(" "));
    }

    [Fact]
    public void ImplicitConversion_WhenDataIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
        {
            AssistantData<TestData> _ = (TestData)null!;
        });
    }
}
