using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.UpdateRecipe;

public sealed class UpdateRecipeHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IEventStoreOperations _events = Substitute.For<IEventStoreOperations>();
    private readonly UpdateRecipeHandler _handler;

    public UpdateRecipeHandlerTests()
    {
        _session.Events.Returns(_events);
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);

        _handler = new UpdateRecipeHandler(Substitute.For<ILogger<UpdateRecipeHandler>>(), _store);
    }

    private static Recipe BuildRecipe(
        Guid id,
        string title = "Chocolate Cake",
        string? sourceUrl = "https://example.com/cake",
        List<Step>? steps = null,
        string notes = "Best served warm."
    )
    {
        var ingredients = new List<Ingredient>
        {
            new(4m, "tablespoons", "granulated sugar"),
            new(1m, "tablespoon", "ground cinnamon"),
        };

        return Recipe
            .Create(id, title, sourceUrl, steps ?? [new Step("Mix and bake.")], ingredients, notes)
            .State;
    }

    private static UpdateRecipeInput ValidInput(
        Guid recipeId,
        string title = "Updated Title",
        string? sourceUrl = "https://example.com/updated",
        IReadOnlyList<StepInput>? steps = null,
        IReadOnlyList<IngredientInput>? ingredients = null,
        string notes = "Updated notes."
    )
    {
        return new UpdateRecipeInput(
            new Id<Recipe>(recipeId.ToString()),
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
    public async Task Handle_WhenHappyPath_ShouldAppendPerFieldEventsAndReturnId()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        var recipe = BuildRecipe(id);
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));

        var input = ValidInput(id);
        object[]? capturedEvents = null;
        Guid? capturedStreamId = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
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
        result.Id.ShouldBe(recipe.RecipeId);

        capturedStreamId.ShouldBe(id);

        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(4);

        var titleChanged = capturedEvents[0].ShouldBeOfType<RecipeTitleChanged>();
        titleChanged.Title.ShouldBe(input.Title);

        var sourceUrlChanged = capturedEvents[1].ShouldBeOfType<RecipeSourceUrlChanged>();
        sourceUrlChanged.SourceUrl.ShouldBe(input.SourceUrl);

        var ingredientsChanged = capturedEvents[2].ShouldBeOfType<RecipeIngredientsChanged>();
        ingredientsChanged.Ingredients.Count.ShouldBe(2);
        ingredientsChanged.Ingredients[0].ShouldBe(new Ingredient(0m, "cups", "flour"));
        ingredientsChanged.Ingredients[1].ShouldBe(new Ingredient(0m, "cups", "sugar"));

        var notesChanged = capturedEvents[3].ShouldBeOfType<RecipeNotesChanged>();
        notesChanged.Notes.ShouldBe(input.Notes);

        await _session.Received(1).SaveChangesAsync(ct);
    }

    [Fact]
    public async Task Handle_WhenStructuredIngredientsProvided_ShouldMapOntoEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var recipe = BuildRecipe(id);
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));

        var input = ValidInput(
            id,
            ingredients:
            [
                new IngredientInput(4m, "tablespoons", "granulated sugar"),
                new IngredientInput(1m, "tablespoon", "ground cinnamon"),
                new IngredientInput(0.5m, "teaspoon", "vanilla extract"),
            ]
        );
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        await _handler.Handle(input, CancellationToken.None);

        // Assert
        capturedEvents.ShouldNotBeNull();
        var ev = capturedEvents.OfType<RecipeIngredientsChanged>().Single();
        ev.Ingredients.Count.ShouldBe(3);
        ev.Ingredients[0].ShouldBe(new Ingredient(4m, "tablespoons", "granulated sugar"));
        ev.Ingredients[1].ShouldBe(new Ingredient(1m, "tablespoon", "ground cinnamon"));
        ev.Ingredients[2].ShouldBe(new Ingredient(0.5m, "teaspoon", "vanilla extract"));
    }

    [Fact]
    public async Task Handle_WhenStructuredStepsProvided_ShouldMapOntoEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var recipe = BuildRecipe(id);
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));

        var input = ValidInput(
            id,
            steps:
            [
                new StepInput("Preheat the oven to 180°C."),
                new StepInput("Mix the dry ingredients in a large bowl."),
                new StepInput("Bake for 30 minutes, then cool on a rack."),
            ]
        );
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        await _handler.Handle(input, CancellationToken.None);

        // Assert
        capturedEvents.ShouldNotBeNull();
        var ev = capturedEvents.OfType<RecipeStepsChanged>().Single();
        ev.Steps.Count.ShouldBe(3);
        ev.Steps[0].ShouldBe(new Step("Preheat the oven to 180°C."));
        ev.Steps[1].ShouldBe(new Step("Mix the dry ingredients in a large bowl."));
        ev.Steps[2].ShouldBe(new Step("Bake for 30 minutes, then cool on a rack."));
    }

    [Fact]
    public async Task Handle_WhenNoFieldsChanged_ShouldNotAppendOrSave()
    {
        // Arrange
        var id = Guid.NewGuid();
        var recipe = BuildRecipe(id);
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));

        var input = new UpdateRecipeInput(
            new Id<Recipe>(id.ToString()),
            recipe.Title,
            recipe.SourceUrl,
            [.. recipe.Steps.Select(s => new StepInput(s.Text))],
            [.. recipe.Ingredients.Select(i => new IngredientInput(i.Quantity, i.Unit, i.Name))],
            recipe.Notes
        );

        // Act
        var result = await _handler.Handle(input, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(recipe.RecipeId);
        _events.DidNotReceive().Append(Arg.Any<Guid>(), Arg.Any<object[]>());
        await _session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(null));

        var input = ValidInput(Guid.NewGuid());

        // Act
        var update = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await update.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenRecipeIdIsNotAGuid_ShouldThrowNotFoundException()
    {
        // Arrange
        var input = new UpdateRecipeInput(
            new Id<Recipe>("not-a-guid"),
            "Updated Title",
            null,
            [new StepInput("Mix and bake.")],
            [new IngredientInput(0m, "cups", "flour")],
            "Updated notes."
        );

        // Act
        var update = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await update.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var input = ValidInput(Guid.NewGuid());
        var ct = new CancellationToken(true);

        // Act
        var update = async () => await _handler.Handle(input, ct);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
