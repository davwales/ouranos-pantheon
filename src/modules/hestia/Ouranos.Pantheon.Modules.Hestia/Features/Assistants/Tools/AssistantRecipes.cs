using System.Globalization;
using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools;

/// <summary>
/// Loads the recipe behind a kitchen assistant context, checks proposed changes against the
/// recipe's own rules and formats ingredients, so the prompt and the tools agree on what the
/// recipe looks like.
/// </summary>
internal static class AssistantRecipes
{
    public const string UnitlessUnit = "whole";

    public static async Task<Recipe> LoadAsync(
        IHestiaMartenStore store,
        IRecipeScope context,
        CancellationToken cancellationToken
    )
    {
        if (!context.RecipeId.TryGetStreamId(out var streamId))
        {
            Guard.Against.NotFound(context.RecipeId, (Recipe?)null);
        }

        using var session = store.QuerySession();
        var recipe = await session.LoadAsync<Recipe>(streamId, cancellationToken);
        return Guard.Against.NotFound(context.RecipeId, recipe);
    }

    public static ProposedIngredient ToProposed(Ingredient ingredient)
    {
        return new ProposedIngredient(
            ingredient.Quantity,
            ingredient.Unit,
            ingredient.Name,
            ingredient.IsOptional
        );
    }

    // A null quantity means "to taste", stored as zero; a blank unit means a countable item.
    public static ProposedIngredient Propose(
        decimal? quantity,
        string unit,
        string name,
        bool isOptional
    )
    {
        return new ProposedIngredient(
            quantity ?? 0,
            string.IsNullOrWhiteSpace(unit) ? UnitlessUnit : unit.Trim(),
            name.Trim(),
            isOptional
        );
    }

    // The domain value type owns the limits, so a proposal is valid exactly when it constructs.
    public static string? Validate(ProposedIngredient ingredient)
    {
        try
        {
            _ = new Ingredient(
                ingredient.Quantity,
                ingredient.Unit,
                ingredient.Name,
                ingredient.IsOptional
            );
            return null;
        }
        catch (ArgumentException ex)
        {
            return $"Invalid ingredient: {ex.Message}";
        }
    }

    public static string? ValidateStep(string text)
    {
        try
        {
            _ = new Step(text);
            return null;
        }
        catch (ArgumentException ex)
        {
            return $"Invalid step: {ex.Message}";
        }
    }

    public static string FormatIngredient(ProposedIngredient ingredient)
    {
        var optional = ingredient.IsOptional ? " (optional)" : "";

        if (ingredient.Quantity <= 0)
        {
            return $"{ingredient.Unit} {ingredient.Name}{optional}";
        }

        var quantity = ingredient.Quantity.ToString("0.##", CultureInfo.InvariantCulture);
        return $"{quantity} {ingredient.Unit} {ingredient.Name}{optional}";
    }

    public static string? NumberProblem(int number, int count, string kind)
    {
        return number >= 1 && number <= count
            ? null
            : $"There is no {kind} {number}; the recipe has {count} {kind}s.";
    }
}
