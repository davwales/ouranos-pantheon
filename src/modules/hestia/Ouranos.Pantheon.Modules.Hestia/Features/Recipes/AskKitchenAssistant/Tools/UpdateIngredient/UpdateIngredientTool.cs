using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateIngredient.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateIngredient;

public sealed class UpdateIngredientTool(
    ILogger<UpdateIngredientTool> logger,
    IHestiaMartenStore store
) : AssistantTool<KitchenAssistantContext, UpdateIngredientArguments>(logger)
{
    internal const string Unchanged = "That would not change the ingredient.";

    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public override string Name => "update_ingredient";

    public override string Description =>
        "Propose replacing ingredient number N, for a substitution, a new amount or marking it "
        + "optional. Give every field of the new ingredient, copying unchanged ones from the "
        + "recipe; a null quantity means 'to taste'.";

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        KitchenAssistantContext context,
        UpdateIngredientArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var recipe = await KitchenRecipes.LoadAsync(_store, context, cancellationToken);
        if (
            KitchenRecipes.NumberProblem(
                arguments.Number,
                recipe.Ingredients.Count,
                "ingredient"
            ) is
            { } numberProblem
        )
        {
            return new AssistantToolResult(numberProblem);
        }

        var original = KitchenRecipes.ToProposed(recipe.Ingredients[arguments.Number - 1]);
        var updated = KitchenRecipes.Propose(
            arguments.Quantity,
            arguments.Unit,
            arguments.Name,
            arguments.Optional
        );

        if (updated == original)
        {
            return new AssistantToolResult(Unchanged);
        }

        if (KitchenRecipes.Validate(updated) is { } problem)
        {
            return new AssistantToolResult(problem);
        }

        return KitchenProposal.Of(
            $"change {KitchenRecipes.FormatIngredient(original)} to "
                + KitchenRecipes.FormatIngredient(updated),
            new UpdateIngredientAction(original, updated)
        );
    }
}
