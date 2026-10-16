using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.UpdateIngredient.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.UpdateIngredient;

public sealed class UpdateIngredientTool(
    ILogger<UpdateIngredientTool> logger,
    IHestiaMartenStore store
) : AssistantTool<IRecipeScope, UpdateIngredientArguments>(logger)
{
    internal const string Unchanged = "That would not change the ingredient.";

    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public override string Name => "update_ingredient";

    public override string Description =>
        "Propose replacing ingredient number N, for a substitution, a new amount or marking it "
        + "optional. Give every field of the new ingredient, copying unchanged ones from the "
        + "recipe; a null quantity means 'to taste'.";

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        IRecipeScope context,
        UpdateIngredientArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var recipe = await AssistantRecipes.LoadAsync(_store, context, cancellationToken);
        if (
            AssistantRecipes.NumberProblem(
                arguments.Number,
                recipe.Ingredients.Count,
                "ingredient"
            ) is
            { } numberProblem
        )
        {
            return new AssistantToolResult(numberProblem);
        }

        var original = AssistantRecipes.ToProposed(recipe.Ingredients[arguments.Number - 1]);
        var updated = AssistantRecipes.Propose(
            arguments.Quantity,
            arguments.Unit,
            arguments.Name,
            arguments.Optional
        );

        if (updated == original)
        {
            return new AssistantToolResult(Unchanged);
        }

        if (AssistantRecipes.Validate(updated) is { } problem)
        {
            return new AssistantToolResult(problem);
        }

        return AssistantProposal.Of(
            $"change {AssistantRecipes.FormatIngredient(original)} to "
                + AssistantRecipes.FormatIngredient(updated),
            new UpdateIngredientAction(original, updated)
        );
    }
}
