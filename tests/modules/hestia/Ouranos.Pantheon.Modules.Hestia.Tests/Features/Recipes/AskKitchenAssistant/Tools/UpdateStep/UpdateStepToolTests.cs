using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateStep;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateStep.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.AskKitchenAssistant.Tools.UpdateStep;

public sealed class UpdateStepToolTests
{
    private readonly KitchenRecipeFixture _fixture = new();
    private readonly UpdateStepTool _tool;

    public UpdateStepToolTests()
    {
        _tool = new UpdateStepTool(Substitute.For<ILogger<UpdateStepTool>>(), _fixture.Store);
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool
            .InvokeAsync(KitchenRecipeFixture.Context, arguments, CancellationToken.None)
            .AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenNumberExists_ShouldProposeTheNewText()
    {
        // Act
        var result = await InvokeAsync("""{"number":2,"text":" Bake for 35 minutes. "}""");

        // Assert
        result.Action.ShouldBe(
            new UpdateStepAction("Bake for 30 minutes.", "Bake for 35 minutes.")
        );
    }

    [Fact]
    public async Task InvokeAsync_WhenNumberIsOutOfRange_ShouldExplainWhy()
    {
        // Act
        var result = await InvokeAsync("""{"number":0,"text":"Rest."}""");

        // Assert
        result.Output.ShouldBe("There is no step 0; the recipe has 2 steps.");
    }

    [Fact]
    public async Task InvokeAsync_WhenTextIsBlank_ShouldExplainWhy()
    {
        // Act
        var result = await InvokeAsync("""{"number":1,"text":"   "}""");

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldStartWith("Invalid step:");
    }

    [Fact]
    public async Task InvokeAsync_WhenTextIsTooLong_ShouldExplainWhy()
    {
        // Act
        var result = await InvokeAsync($$"""{"number":1,"text":"{{new string('x', 2_001)}}"}""");

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldStartWith("Invalid step:");
    }

    [Fact]
    public void Constructor_WhenStoreIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new UpdateStepTool(Substitute.For<ILogger<UpdateStepTool>>(), null!)
        );
    }
}
