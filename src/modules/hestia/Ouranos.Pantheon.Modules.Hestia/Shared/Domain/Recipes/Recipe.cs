using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;

public sealed record Recipe : BaseEventSourcedEntity
{
    [JsonConstructor]
    private Recipe() { }

    public Id<Recipe> RecipeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? SourceUrl { get; init; }
    public List<Step> Steps { get; init; } = [];
    public List<Ingredient> Ingredients { get; init; } = [];
    public string Notes { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public RecipeImportStatus ImportStatus { get; init; }
    public string? ImportFailureReason { get; init; }

    /// <summary>
    /// Marten convention: project a <see cref="RecipeCreated"/> event into the
    /// initial aggregate state. No validation - events are historical truth.
    /// </summary>
    public static Recipe Create(RecipeCreated @event)
    {
        return new()
        {
            Id = @event.Id,
            RecipeId = new Id<Recipe>(@event.Id.ToString()),
            Title = @event.Title,
            SourceUrl = @event.SourceUrl,
            Steps = @event.Steps,
            Ingredients = @event.Ingredients,
            Notes = @event.Notes,
            CreatedAt = @event.CreatedAt,
            ImportStatus = RecipeImportStatus.None,
        };
    }

    /// <summary>
    /// Command factory: validates inputs, emits a <see cref="RecipeCreated"/>
    /// event, and returns the resulting state. This is the only public path
    /// to construct a new <see cref="Recipe"/> from command inputs.
    /// </summary>
    public static OperationResult<Recipe> Create(
        Guid id,
        string title,
        string? sourceUrl,
        List<Step> steps,
        List<Ingredient> ingredients,
        string notes
    )
    {
        GuardValidFields(title, sourceUrl, steps, ingredients, notes);

        var @event = new RecipeCreated(
            id,
            title,
            sourceUrl,
            steps,
            ingredients,
            notes,
            DateTimeOffset.UtcNow
        );

        return OperationResult<Recipe>.Of(Create(@event), @event);
    }

    /// <summary>
    /// Command factory: creates a recipe that is pending import from an external
    /// source. Emits a <see cref="RecipeCreated"/> event with placeholder content;
    /// the import pipeline later appends a <see cref="RecipeImportSucceeded"/> or
    /// <see cref="RecipeImportFailed"/> event.
    /// </summary>
    public static OperationResult<Recipe> CreateImport(
        Guid id,
        string sourceUrl,
        DateTimeOffset startedAt
    )
    {
        Guard.Against.NullOrWhiteSpace(sourceUrl);
        Guard.Against.OutOfRange(sourceUrl.Length, nameof(sourceUrl), 1, 2_000);

        var created = new RecipeCreated(
            id,
            "New Recipe",
            sourceUrl,
            [],
            [],
            string.Empty,
            startedAt
        );
        var started = new RecipeImportStarted(startedAt);

        return OperationResult<Recipe>.Of(Apply(started, Create(created)), created, started);
    }

    /// <summary>
    /// Command factory: validates the proposed fields and emits a
    /// <see cref="IDomainEvent"/> per field whose value differs from the
    /// current state, returning the resulting state. No-op fields produce no
    /// events, so the event history shows exactly what changed.
    /// </summary>
    public OperationResult<Recipe> Update(
        string title,
        string? sourceUrl,
        List<Step> steps,
        List<Ingredient> ingredients,
        string notes
    )
    {
        GuardValidFields(title, sourceUrl, steps, ingredients, notes);

        var events = new List<IDomainEvent>();
        var state = this;

        if (title != Title)
        {
            var @event = new RecipeTitleChanged(title);
            events.Add(@event);
            state = Apply(@event, state);
        }

        if (sourceUrl != SourceUrl)
        {
            var @event = new RecipeSourceUrlChanged(sourceUrl);
            events.Add(@event);
            state = Apply(@event, state);
        }

        if (!steps.SequenceEqual(Steps))
        {
            var @event = new RecipeStepsChanged(steps);
            events.Add(@event);
            state = Apply(@event, state);
        }

        if (!ingredients.SequenceEqual(Ingredients))
        {
            var @event = new RecipeIngredientsChanged(ingredients);
            events.Add(@event);
            state = Apply(@event, state);
        }

        if (notes != Notes)
        {
            var @event = new RecipeNotesChanged(notes);
            events.Add(@event);
            state = Apply(@event, state);
        }

        return OperationResult<Recipe>.Of(state, [.. events]);
    }

    private static void GuardValidFields(
        string title,
        string? sourceUrl,
        List<Step> steps,
        List<Ingredient> ingredients,
        string notes
    )
    {
        Guard.Against.NullOrWhiteSpace(title);
        Guard.Against.OutOfRange(title.Length, nameof(title), 1, 200);
        Guard.Against.Null(steps);
        Guard.Against.OutOfRange(steps.Count, nameof(steps), 1, 100);
        Guard.Against.Null(ingredients);
        Guard.Against.OutOfRange(ingredients.Count, nameof(ingredients), 1, 100);

        if (!string.IsNullOrWhiteSpace(sourceUrl))
        {
            Guard.Against.OutOfRange(sourceUrl.Length, nameof(sourceUrl), 1, 2_000);
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Guard.Against.OutOfRange(notes.Length, nameof(notes), 1, 10_000);
        }
    }

    /// <summary>
    /// Command factory: reverts the recipe to a historical state by emitting a
    /// <see cref="RecipeReverted"/> event carrying the state to restore. The
    /// historical state originates from a Marten live aggregation at the target
    /// version, so it was already valid when first written and is not re-validated.
    /// </summary>
    public OperationResult<Recipe> Revert(
        long targetVersion,
        Recipe historical,
        DateTimeOffset revertedAt
    )
    {
        var @event = new RecipeReverted(
            targetVersion,
            historical.Title,
            historical.SourceUrl,
            historical.Steps,
            historical.Ingredients,
            historical.Notes,
            revertedAt
        );

        return OperationResult<Recipe>.Of(Apply(@event, this), @event);
    }

    /// <summary>
    /// Command factory: records a successful import by emitting a
    /// <see cref="RecipeImportSucceeded"/> event carrying the imported content.
    /// </summary>
    public OperationResult<Recipe> CompleteImport(
        string title,
        List<Step> steps,
        List<Ingredient> ingredients,
        string notes
    )
    {
        GuardValidFields(title, null, steps, ingredients, notes);

        var @event = new RecipeImportSucceeded(
            title,
            steps,
            ingredients,
            notes,
            DateTimeOffset.UtcNow
        );

        return OperationResult<Recipe>.Of(Apply(@event, this), @event);
    }

    /// <summary>
    /// Command factory: records a failed import by emitting a
    /// <see cref="RecipeImportFailed"/> event carrying the failure reason.
    /// </summary>
    public OperationResult<Recipe> FailImport(string reason)
    {
        Guard.Against.NullOrWhiteSpace(reason);

        var @event = new RecipeImportFailed(reason, DateTimeOffset.UtcNow);

        return OperationResult<Recipe>.Of(Apply(@event, this), @event);
    }

    /// <summary>
    /// Command factory: re-imports the recipe from its source URL by emitting a
    /// <see cref="RecipeImportStarted"/> event, resetting the import status to
    /// importing. Requires the recipe to have a source URL.
    /// </summary>
    public OperationResult<Recipe> Reimport(DateTimeOffset startedAt)
    {
        if (string.IsNullOrWhiteSpace(SourceUrl))
        {
            throw new InvalidOperationException("Recipe has no source URL to reimport from.");
        }

        var @event = new RecipeImportStarted(startedAt);

        return OperationResult<Recipe>.Of(Apply(@event, this), @event);
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeCreated"/> event to the current state (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeCreated @event, Recipe current)
    {
        return current with
        {
            RecipeId = new Id<Recipe>(@event.Id.ToString()),
            Title = @event.Title,
            SourceUrl = @event.SourceUrl,
            Steps = @event.Steps,
            Ingredients = @event.Ingredients,
            Notes = @event.Notes,
            CreatedAt = @event.CreatedAt,
        };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeTitleChanged"/> event to the current state
    /// (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeTitleChanged @event, Recipe current)
    {
        return current with { Title = @event.Title };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeSourceUrlChanged"/> event to the current state
    /// (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeSourceUrlChanged @event, Recipe current)
    {
        return current with { SourceUrl = @event.SourceUrl };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeStepsChanged"/> event to the current state
    /// (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeStepsChanged @event, Recipe current)
    {
        return current with { Steps = @event.Steps };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeIngredientsChanged"/> event to the current state
    /// (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeIngredientsChanged @event, Recipe current)
    {
        return current with { Ingredients = @event.Ingredients };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeNotesChanged"/> event to the current state
    /// (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeNotesChanged @event, Recipe current)
    {
        return current with { Notes = @event.Notes };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeReverted"/> event to the current state
    /// (functional fold). No validation - events are historical truth.
    /// </summary>
    public static Recipe Apply(RecipeReverted @event, Recipe current)
    {
        return current with
        {
            Title = @event.Title,
            SourceUrl = @event.SourceUrl,
            Steps = @event.Steps,
            Ingredients = @event.Ingredients,
            Notes = @event.Notes,
        };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeImportStarted"/> event to the current state
    /// (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeImportStarted _, Recipe current)
    {
        return current with
        {
            ImportStatus = RecipeImportStatus.Importing,
            ImportFailureReason = null,
        };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeImportSucceeded"/> event to the current state
    /// (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeImportSucceeded @event, Recipe current)
    {
        return current with
        {
            Title = @event.Title,
            Steps = @event.Steps,
            Ingredients = @event.Ingredients,
            Notes = @event.Notes,
            ImportStatus = RecipeImportStatus.Imported,
            ImportFailureReason = null,
        };
    }

    /// <summary>
    /// Marten convention: evolve the aggregate by applying a
    /// <see cref="RecipeImportFailed"/> event to the current state
    /// (functional fold).
    /// </summary>
    public static Recipe Apply(RecipeImportFailed @event, Recipe current)
    {
        return current with
        {
            ImportStatus = RecipeImportStatus.Failed,
            ImportFailureReason = @event.Reason,
        };
    }
}
