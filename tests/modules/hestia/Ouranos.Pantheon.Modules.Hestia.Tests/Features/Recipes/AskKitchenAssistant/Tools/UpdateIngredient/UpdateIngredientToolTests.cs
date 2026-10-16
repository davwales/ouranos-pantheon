using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateIngredient;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateIngredient.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.AskKitchenAssistant.Tools.UpdateIngredient;

public sealed class UpdateIngredientToolTests
{
    private readonly KitchenRecipeFixture _fixture = new();
    private readonly UpdateIngredientTool _tool;

    public UpdateIngredientToolTests()
    {
        _tool = new UpdateIngredientTool(
            Substitute.For<ILogger<UpdateIngredientTool>>(),
            _fixture.Store
        );
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool
            .InvokeAsync(KitchenRecipeFixture.Context, arguments, CancellationToken.None)
            .AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenMarkingOptional_ShouldProposeTheReplacement()
    {
        // Act
        var result = await InvokeAsync(
            """{"number":2,"quantity":2,"unit":"tbsp","name":"parsley","optional":true}"""
        );

        // Assert
        result.Action.ShouldBe(
            new UpdateIngredientAction(
                new ProposedIngredient(2, "tbsp", "parsley", false),
                new ProposedIngredient(2, "tbsp", "parsley", true)
            )
        );
        result.Output.ShouldStartWith(
            "Proposed to the user: change 2 tbsp parsley to 2 tbsp parsley (optional)"
        );
    }

    [Fact]
    public async Task InvokeAsync_WhenSubstituting_ShouldProposeTheNewIngredient()
    {
        // Act
        var result = await InvokeAsync(
            """{"number":2,"quantity":2,"unit":"tbsp","name":"cilantro","optional":false}"""
        );

        // Assert
        result
            .Action.ShouldBeOfType<UpdateIngredientAction>()
            .Updated.ShouldBe(new ProposedIngredient(2, "tbsp", "cilantro", false));
    }

    [Theory]
    [InlineData(
        """{"number":3,"quantity":1,"unit":"g","name":"salt","optional":true}""",
        "There is no ingredient 3; the recipe has 2 ingredients."
    )]
    [InlineData(
        """{"number":1,"quantity":1.5,"unit":"cups","name":"flour","optional":false}""",
        UpdateIngredientTool.Unchanged
    )]
    public async Task InvokeAsync_WhenNothingValidChanges_ShouldNotPropose(
        string arguments,
        string output
    )
    {
        // Act
        var result = await InvokeAsync(arguments);

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldBe(output);
    }

    [Fact]
    public async Task InvokeAsync_WhenTheReplacementBreaksTheRecipeRules_ShouldExplainWhy()
    {
        // Act
        var result = await InvokeAsync(
            """{"number":1,"quantity":-2,"unit":"cups","name":"flour","optional":false}"""
        );

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldStartWith("Invalid ingredient:");
    }

    [Fact]
    public void Constructor_WhenStoreIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new UpdateIngredientTool(Substitute.For<ILogger<UpdateIngredientTool>>(), null!)
        );
    }
}
