using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddIngredient;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddIngredient.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.AskKitchenAssistant.Tools.AddIngredient;

public sealed class AddIngredientToolTests
{
    private readonly AddIngredientTool _tool = new(Substitute.For<ILogger<AddIngredientTool>>());

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool
            .InvokeAsync(KitchenRecipeFixture.Context, arguments, CancellationToken.None)
            .AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenIngredientIsValid_ShouldProposeIt()
    {
        // Act
        var result = await InvokeAsync(
            """{"quantity":200,"unit":" g ","name":" sugar ","optional":false}"""
        );

        // Assert
        result.Action.ShouldBe(
            new AddIngredientAction(new ProposedIngredient(200, "g", "sugar", false))
        );
        result.Output.ShouldStartWith("Proposed to the user: add 200 g sugar.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnitIsBlankAndQuantityIsNull_ShouldUseTheDefaults()
    {
        // Act
        var result = await InvokeAsync(
            """{"quantity":null,"unit":"","name":"eggs","optional":true}"""
        );

        // Assert
        result.Action.ShouldBe(
            new AddIngredientAction(
                new ProposedIngredient(0, KitchenRecipes.UnitlessUnit, "eggs", true)
            )
        );
    }

    [Fact]
    public async Task InvokeAsync_WhenIngredientBreaksTheRecipeRules_ShouldExplainWhy()
    {
        // Act
        var result = await InvokeAsync(
            """{"quantity":-1,"unit":"g","name":"salt","optional":false}"""
        );

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldStartWith("Invalid ingredient:");
    }
}
