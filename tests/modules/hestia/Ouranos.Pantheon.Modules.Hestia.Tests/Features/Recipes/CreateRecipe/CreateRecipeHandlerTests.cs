using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.CreateRecipe;

public sealed class CreateRecipeHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IEventStoreOperations _events = Substitute.For<IEventStoreOperations>();
    private readonly CreateRecipeHandler _handler;

    public CreateRecipeHandlerTests()
    {
        _session.Events.Returns(_events);
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);

        _handler = new CreateRecipeHandler(Substitute.For<ILogger<CreateRecipeHandler>>(), _store);
    }

    private static CreateRecipeInput ValidInput(
        string title = "Chocolate Cake",
        string? sourceUrl = "https://example.com/cake",
        IReadOnlyList<StepInput>? steps = null,
        IReadOnlyList<IngredientInput>? ingredients = null,
        string notes = "Best served warm."
    )
    {
        return new CreateRecipeInput(
            title,
            sourceUrl,
            steps ?? [new StepInput("Mix and bake.")],
            ingredients
                ??
                [
                    new IngredientInput(0m, "cups", "flour"),
                    new IngredientInput(0m, "cups", "sugar"),
                ],
            notes
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCommitEventsAndReturnId()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = ValidInput();
        object[]? capturedEvents = null;
        Guid? capturedStreamId = null;
        _events
            .When(e => e.StartStream(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call =>
            {
                capturedStreamId = call.Arg<Guid>();
                capturedEvents = call.Arg<object[]>();
            });

        // Act
        var result = await _handler.Handle(input, ct);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBeOfType<Id<Recipe>>();
        result.Id.Value.ShouldNotBeNullOrEmpty();
        Guid.Parse(result.Id.Value).ShouldNotBe(Guid.Empty);
        capturedStreamId.ShouldNotBeNull();
        capturedStreamId.Value.ShouldNotBe(Guid.Empty);

        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var ev = capturedEvents[0].ShouldBeOfType<RecipeCreated>();
        ev.Title.ShouldBe(input.Title);
        ev.SourceUrl.ShouldBe(input.SourceUrl);
        ev.Steps.Count.ShouldBe(1);
        ev.Steps[0].Text.ShouldBe("Mix and bake.");
        ev.Notes.ShouldBe(input.Notes);
        ev.Ingredients.Count.ShouldBe(2);
        ev.Ingredients[0].ShouldBe(new Ingredient(0m, "cups", "flour"));
        ev.Ingredients[1].ShouldBe(new Ingredient(0m, "cups", "sugar"));

        await _session.Received(1).SaveChangesAsync(ct);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var input = ValidInput();
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await _handler.Handle(input, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenIngredientsProvided_ShouldMapStructuredIngredients()
    {
        // Arrange
        var input = ValidInput(
            ingredients:
            [
                new IngredientInput(4m, "tablespoons", "granulated sugar"),
                new IngredientInput(1m, "tablespoon", "ground cinnamon"),
                new IngredientInput(0.5m, "teaspoon", "vanilla extract"),
            ]
        );
        object[]? capturedEvents = null;
        _events
            .When(e => e.StartStream(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        await _handler.Handle(input, CancellationToken.None);

        // Assert
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var ev = capturedEvents[0].ShouldBeOfType<RecipeCreated>();
        ev.Ingredients.Count.ShouldBe(3);
        ev.Ingredients[0].ShouldBe(new Ingredient(4m, "tablespoons", "granulated sugar"));
        ev.Ingredients[1].ShouldBe(new Ingredient(1m, "tablespoon", "ground cinnamon"));
        ev.Ingredients[2].ShouldBe(new Ingredient(0.5m, "teaspoon", "vanilla extract"));
    }

    [Fact]
    public async Task Handle_WhenStepsProvided_ShouldMapStructuredSteps()
    {
        // Arrange
        var input = ValidInput(
            steps:
            [
                new StepInput("Preheat the oven to 350°F."),
                new StepInput("Mix the dry ingredients in a large bowl."),
                new StepInput("Bake for 30 minutes, then cool on a rack."),
            ]
        );
        object[]? capturedEvents = null;
        _events
            .When(e => e.StartStream(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        await _handler.Handle(input, CancellationToken.None);

        // Assert
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var ev = capturedEvents[0].ShouldBeOfType<RecipeCreated>();
        ev.Steps.Count.ShouldBe(3);
        ev.Steps[0].ShouldBe(new Step("Preheat the oven to 350°F."));
        ev.Steps[1].ShouldBe(new Step("Mix the dry ingredients in a large bowl."));
        ev.Steps[2].ShouldBe(new Step("Bake for 30 minutes, then cool on a rack."));
    }

    [Fact]
    public async Task Handle_WhenIngredientsExceedsMaxCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var ingredients = Enumerable
            .Range(0, 101)
            .Select(_ => new IngredientInput(0m, "cups", "flour"))
            .ToList();
        var input = ValidInput(ingredients: ingredients);

        // Act
        var act = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }
}
