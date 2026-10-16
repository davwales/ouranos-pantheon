using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.RemoveIngredient;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.RemoveIngredient.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Hestia.Tests.Features.Assistants.AskKitchenAssistant;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Assistants.Tools.RemoveIngredient;

public sealed class RemoveIngredientToolTests
{
    private readonly KitchenRecipeFixture _fixture = new();
    private readonly RemoveIngredientTool _tool;

    public RemoveIngredientToolTests()
    {
        _tool = new RemoveIngredientTool(
            Substitute.For<ILogger<RemoveIngredientTool>>(),
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
    public async Task InvokeAsync_WhenNumberExists_ShouldProposeRemovingTheOriginal()
    {
        // Act
        var result = await InvokeAsync("""{"number":1}""");

        // Assert
        result.Action.ShouldBe(
            new RemoveIngredientAction(new ProposedIngredient(1.5m, "cups", "flour", false))
        );
        result.Output.ShouldStartWith("Proposed to the user: remove 1.5 cups flour.");
    }

    [Fact]
    public async Task InvokeAsync_WhenNumberIsOutOfRange_ShouldExplainWhy()
    {
        // Act
        var result = await InvokeAsync("""{"number":3}""");

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldBe("There is no ingredient 3; the recipe has 2 ingredients.");
    }

    [Fact]
    public async Task InvokeAsync_WhenItIsTheOnlyIngredient_ShouldRefuse()
    {
        // Arrange
        _fixture.Returns(
            KitchenRecipeFixture.BuildRecipe() with
            {
                Ingredients = [new Ingredient(1, "cup", "flour")],
            }
        );

        // Act
        var result = await InvokeAsync("""{"number":1}""");

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldBe("A recipe needs at least one ingredient.");
    }

    [Fact]
    public void Constructor_WhenStoreIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new RemoveIngredientTool(Substitute.For<ILogger<RemoveIngredientTool>>(), null!)
        );
    }
}
