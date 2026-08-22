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
}
