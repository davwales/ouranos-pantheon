using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddNote;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddNote.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.AskKitchenAssistant.Tools.AddNote;

public sealed class AddNoteToolTests
{
    private readonly KitchenRecipeFixture _fixture = new();
    private readonly AddNoteTool _tool;

    public AddNoteToolTests()
    {
        _tool = new AddNoteTool(Substitute.For<ILogger<AddNoteTool>>(), _fixture.Store);
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool
            .InvokeAsync(KitchenRecipeFixture.Context, arguments, CancellationToken.None)
            .AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoteFits_ShouldProposeIt()
    {
        // Act
        var result = await InvokeAsync("""{"text":" Freezes well. "}""");

        // Assert
        result.Action.ShouldBe(new AddRecipeNoteAction("Freezes well."));
        result.Output.ShouldStartWith("Proposed to the user: add the note \"Freezes well.\"");
    }

    [Fact]
    public async Task InvokeAsync_WhenNoteIsEmpty_ShouldRefuse()
    {
        // Act
        var result = await InvokeAsync("""{"text":"  "}""");

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldBe("The note is empty.");
    }

    [Fact]
    public async Task InvokeAsync_WhenNotesWouldBeTooLong_ShouldRefuse()
    {
        // Arrange
        _fixture.Returns(
            KitchenRecipeFixture.BuildRecipe(new string('n', Recipe.MaxNotesLength - 5))
        );

        // Act
        var result = await InvokeAsync("""{"text":"Too long"}""");

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldBe("The notes would be too long; propose a shorter note.");
    }

    [Fact]
    public void Constructor_WhenStoreIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new AddNoteTool(Substitute.For<ILogger<AddNoteTool>>(), null!)
        );
    }
}
