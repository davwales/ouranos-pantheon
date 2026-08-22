using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Shared.Domain.Recipes;

public sealed class RecipeTests
{
    private static List<Ingredient> ValidIngredients()
    {
        return [new Ingredient(0m, "cups", "flour")];
    }

    private static List<Step> ValidSteps()
    {
        return [new Step("Mix and bake.")];
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldReturnRecipeWithCorrectState()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = ValidIngredients();
        var steps = ValidSteps();

        // Act
        var result = Recipe.Create(
            id,
            "Chocolate Cake",
            "https://example.com/cake",
            steps,
            ingredients,
            "Best served warm."
        );

        // Assert
        result.State.Id.ShouldBe(id);
        result.State.Title.ShouldBe("Chocolate Cake");
        result.State.SourceUrl.ShouldBe("https://example.com/cake");
        result.State.Steps.ShouldBe(steps);
        result.State.Ingredients.ShouldBe(ingredients);
        result.State.Notes.ShouldBe("Best served warm.");
        result.State.CreatedAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);

        var @event = result.Events[0].ShouldBeOfType<RecipeCreated>();
        @event.Id.ShouldBe(id);
        @event.Title.ShouldBe("Chocolate Cake");
        @event.SourceUrl.ShouldBe("https://example.com/cake");
        @event.Steps.ShouldBe(steps);
        @event.Ingredients.ShouldBe(ingredients);
        @event.Notes.ShouldBe("Best served warm.");
        @event.CreatedAt.ShouldBe(result.State.CreatedAt);
    }

    [Fact]
    public void Create_WhenProjectingEvent_ShouldReturnRecipeMatchingEvent()
    {
        // Arrange
        var @event = new RecipeCreated(
            Guid.NewGuid(),
            "Chocolate Cake",
            "https://example.com/cake",
            ValidSteps(),
            ValidIngredients(),
            "Best served warm.",
            DateTimeOffset.UtcNow
        );

        // Act
        var recipe = Recipe.Create(@event);

        // Assert
        recipe.Id.ShouldBe(@event.Id);
        recipe.RecipeId.ShouldBe(new Id<Recipe>(@event.Id.ToString()));
        recipe.Title.ShouldBe(@event.Title);
        recipe.SourceUrl.ShouldBe(@event.SourceUrl);
        recipe.Steps.ShouldBe(@event.Steps);
        recipe.Ingredients.ShouldBe(@event.Ingredients);
        recipe.Notes.ShouldBe(@event.Notes);
        recipe.CreatedAt.ShouldBe(@event.CreatedAt);
    }

    [Fact]
    public void Apply_WhenApplyingEvent_ShouldReturnRecipeMatchingEvent()
    {
        // Arrange
        var @event = new RecipeCreated(
            Guid.NewGuid(),
            "Chocolate Cake",
            "https://example.com/cake",
            ValidSteps(),
            ValidIngredients(),
            "Best served warm.",
            DateTimeOffset.UtcNow
        );
        var current = Recipe.Create(@event);

        // Act
        var result = Recipe.Apply(@event, current);

        // Assert
        result.Id.ShouldBe(@event.Id);
        result.RecipeId.ShouldBe(new Id<Recipe>(@event.Id.ToString()));
        result.Title.ShouldBe(@event.Title);
        result.SourceUrl.ShouldBe(@event.SourceUrl);
        result.Steps.ShouldBe(@event.Steps);
        result.Ingredients.ShouldBe(@event.Ingredients);
        result.Notes.ShouldBe(@event.Notes);
        result.CreatedAt.ShouldBe(@event.CreatedAt);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenTitleIsNullOrWhitespace_ShouldThrowArgumentException(string? title)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            Recipe.Create(id, title!, "https://example.com", ValidSteps(), ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenTitleExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = new string('a', 201);

        // Act
        Action act = () => Recipe.Create(id, title, null, ValidSteps(), ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WhenStepsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () => Recipe.Create(id, "Title", null, null!, ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WhenStepsIsEmptyList_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var steps = new List<Step>();

        // Act
        Action act = () => Recipe.Create(id, "Title", null, steps, ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WhenStepsExceedsMaxCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var steps = Enumerable.Range(0, 101).Select(_ => new Step("Mix.")).ToList();

        // Act
        Action act = () => Recipe.Create(id, "Title", null, steps, ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WhenIngredientsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () => Recipe.Create(id, "Title", null, ValidSteps(), null!, "");

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WhenIngredientsIsEmptyList_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = new List<Ingredient>();

        // Act
        Action act = () => Recipe.Create(id, "Title", null, ValidSteps(), ingredients, "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WhenIngredientsExceedsMaxCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = Enumerable
            .Range(0, 101)
            .Select(_ => new Ingredient(0m, "cups", "flour"))
            .ToList();

        // Act
        Action act = () => Recipe.Create(id, "Title", null, ValidSteps(), ingredients, "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WhenSourceUrlExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sourceUrl = new string('a', 2_001);

        // Act
        Action act = () =>
            Recipe.Create(id, "Title", sourceUrl, ValidSteps(), ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WhenNotesExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var notes = new string('a', 10_001);

        // Act
        Action act = () =>
            Recipe.Create(id, "Title", null, ValidSteps(), ValidIngredients(), notes);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WhenSourceUrlIsNull_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = ValidIngredients();
        var steps = ValidSteps();

        // Act
        var result = Recipe.Create(id, "Title", null, steps, ingredients, "");

        // Assert
        result.State.Id.ShouldBe(id);
        result.State.SourceUrl.ShouldBeNull();
        result.Events[0].ShouldBeOfType<RecipeCreated>().SourceUrl.ShouldBeNull();
    }

    [Fact]
    public void Create_WhenNotesIsEmpty_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = ValidIngredients();
        var steps = ValidSteps();

        // Act
        var result = Recipe.Create(id, "Title", null, steps, ingredients, "");

        // Assert
        result.State.Id.ShouldBe(id);
        result.State.Notes.ShouldBe(string.Empty);
        result.Events[0].ShouldBeOfType<RecipeCreated>().Notes.ShouldBe(string.Empty);
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldRoundTripRecipeIdFromId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = Recipe.Create(
            id,
            "Chocolate Cake",
            null,
            ValidSteps(),
            ValidIngredients(),
            ""
        );

        // Assert
        result.State.Id.ShouldBe(id);
        result.State.RecipeId.ShouldBe(new Id<Recipe>(id.ToString()));
        Guid.Parse(result.State.RecipeId.Value).ShouldBe(id);
        result.Events[0].ShouldBeOfType<RecipeCreated>().Id.ShouldBe(id);
    }

    [Fact]
    public void Create_WhenSourceUrlIsWhitespace_ShouldSucceedAndPreserveValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ingredients = ValidIngredients();
        var steps = ValidSteps();
        var sourceUrl = "   ";

        // Act
        var result = Recipe.Create(id, "Title", sourceUrl, steps, ingredients, "");

        // Assert
        result.State.Id.ShouldBe(id);
        result.State.SourceUrl.ShouldBe(sourceUrl);
        result.Events[0].ShouldBeOfType<RecipeCreated>().SourceUrl.ShouldBe(sourceUrl);
    }

    private static Recipe ValidCurrent()
    {
        return Recipe
            .Create(
                Guid.NewGuid(),
                "Original Title",
                "https://example.com/original",
                [new Step("Mix.")],
                [new Ingredient(0m, "cup", "flour")],
                "Original notes."
            )
            .State;
    }

    [Fact]
    public void Update_WhenAllFieldsChanged_ShouldEmitEveryPerFieldEvent()
    {
        // Arrange
        var current = ValidCurrent();
        var newSteps = new List<Step> { new("Bake at 180°C."), new("Cool before serving.") };
        var newIngredients = new List<Ingredient>
        {
            new(4m, "tablespoons", "granulated sugar"),
            new(1m, "tablespoon", "ground cinnamon"),
        };

        // Act
        var result = current.Update(
            "Updated Title",
            "https://example.com/updated",
            newSteps,
            newIngredients,
            "Updated notes."
        );

        // Assert
        result.State.Id.ShouldBe(current.Id);
        result.State.RecipeId.ShouldBe(current.RecipeId);
        result.State.CreatedAt.ShouldBe(current.CreatedAt);
        result.State.Title.ShouldBe("Updated Title");
        result.State.SourceUrl.ShouldBe("https://example.com/updated");
        result.State.Steps.ShouldBe(newSteps);
        result.State.Ingredients.ShouldBe(newIngredients);
        result.State.Notes.ShouldBe("Updated notes.");

        result.Events.Count.ShouldBe(5);

        var titleChanged = result.Events[0].ShouldBeOfType<RecipeTitleChanged>();
        titleChanged.Title.ShouldBe("Updated Title");

        var sourceUrlChanged = result.Events[1].ShouldBeOfType<RecipeSourceUrlChanged>();
        sourceUrlChanged.SourceUrl.ShouldBe("https://example.com/updated");

        var stepsChanged = result.Events[2].ShouldBeOfType<RecipeStepsChanged>();
        stepsChanged.Steps.ShouldBe(newSteps);

        var ingredientsChanged = result.Events[3].ShouldBeOfType<RecipeIngredientsChanged>();
        ingredientsChanged.Ingredients.ShouldBe(newIngredients);

        var notesChanged = result.Events[4].ShouldBeOfType<RecipeNotesChanged>();
        notesChanged.Notes.ShouldBe("Updated notes.");
    }

    [Fact]
    public void Update_WhenOnlyTitleChanged_ShouldEmitSingleTitleChangedEvent()
    {
        // Arrange
        var current = ValidCurrent();

        // Act
        var result = current.Update(
            "Current Title",
            current.SourceUrl,
            current.Steps,
            current.Ingredients,
            current.Notes
        );

        // Assert
        result.Events.Count.ShouldBe(1);
        var @event = result.Events[0].ShouldBeOfType<RecipeTitleChanged>();
        @event.Title.ShouldBe("Current Title");
        result.State.Title.ShouldBe("Current Title");
        result.State.Steps.ShouldBe(current.Steps);
        result.State.Ingredients.ShouldBe(current.Ingredients);
    }

    [Fact]
    public void Update_WhenNoFieldsChanged_ShouldNotEmitAnyEvents()
    {
        // Arrange
        var current = ValidCurrent();

        // Act
        var result = current.Update(
            current.Title,
            current.SourceUrl,
            current.Steps,
            current.Ingredients,
            current.Notes
        );

        // Assert
        result.Events.ShouldBeEmpty();
        result.State.ShouldBe(current);
    }

    [Fact]
    public void Apply_WhenApplyingRecipeTitleChangedEvent_ShouldReturnUpdatedTitle()
    {
        // Arrange
        var created = new RecipeCreated(
            Guid.NewGuid(),
            "Original Title",
            null,
            ValidSteps(),
            ValidIngredients(),
            "Original notes.",
            DateTimeOffset.UtcNow
        );
        var current = Recipe.Create(created);
        var @event = new RecipeTitleChanged("Updated Title");

        // Act
        var result = Recipe.Apply(@event, current);

        // Assert
        result.Id.ShouldBe(current.Id);
        result.RecipeId.ShouldBe(current.RecipeId);
        result.CreatedAt.ShouldBe(current.CreatedAt);
        result.Title.ShouldBe("Updated Title");
        result.SourceUrl.ShouldBe(current.SourceUrl);
        result.Steps.ShouldBe(current.Steps);
        result.Ingredients.ShouldBe(current.Ingredients);
        result.Notes.ShouldBe(current.Notes);
    }

    [Fact]
    public void Apply_WhenApplyingRecipeIngredientsChangedEvent_ShouldReturnUpdatedIngredients()
    {
        // Arrange
        var created = new RecipeCreated(
            Guid.NewGuid(),
            "Original Title",
            null,
            ValidSteps(),
            ValidIngredients(),
            "Original notes.",
            DateTimeOffset.UtcNow
        );
        var current = Recipe.Create(created);
        var newIngredients = new List<Ingredient> { new(4m, "tablespoons", "granulated sugar") };
        var @event = new RecipeIngredientsChanged(newIngredients);

        // Act
        var result = Recipe.Apply(@event, current);

        // Assert
        result.Id.ShouldBe(current.Id);
        result.Ingredients.ShouldBe(newIngredients);
        result.Title.ShouldBe(current.Title);
        result.Steps.ShouldBe(current.Steps);
        result.Notes.ShouldBe(current.Notes);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WhenTitleIsNullOrWhitespace_ShouldThrowArgumentException(string? title)
    {
        // Arrange
        var current = ValidCurrent();

        // Act
        Action act = () =>
            current.Update(title!, "https://example.com", ValidSteps(), ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenTitleExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var current = ValidCurrent();
        var title = new string('a', 201);

        // Act
        Action act = () => current.Update(title, null, ValidSteps(), ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_WhenStepsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var current = ValidCurrent();

        // Act
        Action act = () => current.Update("Title", null, null!, ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Update_WhenStepsExceedsMaxCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var current = ValidCurrent();
        var steps = Enumerable.Range(0, 101).Select(_ => new Step("Mix.")).ToList();

        // Act
        Action act = () => current.Update("Title", null, steps, ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_WhenIngredientsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var current = ValidCurrent();

        // Act
        Action act = () => current.Update("Title", null, ValidSteps(), null!, "");

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Update_WhenIngredientsExceedsMaxCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var current = ValidCurrent();
        var ingredients = Enumerable
            .Range(0, 101)
            .Select(_ => new Ingredient(0m, "cups", "flour"))
            .ToList();

        // Act
        Action act = () => current.Update("Title", null, ValidSteps(), ingredients, "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_WhenSourceUrlExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var current = ValidCurrent();
        var sourceUrl = new string('a', 2_001);

        // Act
        Action act = () => current.Update("Title", sourceUrl, ValidSteps(), ValidIngredients(), "");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_WhenNotesExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var current = ValidCurrent();
        var notes = new string('a', 10_001);

        // Act
        Action act = () => current.Update("Title", null, ValidSteps(), ValidIngredients(), notes);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }
}
