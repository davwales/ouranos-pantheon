using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.RemoveIngredient.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.RemoveIngredient;

public sealed class RemoveIngredientTool(
    ILogger<RemoveIngredientTool> logger,
    IHestiaMartenStore store
) : AssistantTool<IRecipeScope, RemoveIngredientArguments>(logger)
{
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public override string Name => "remove_ingredient";

    public override string Description => "Propose removing ingredient number N from the recipe.";

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        IRecipeScope context,
        RemoveIngredientArguments arguments,
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

        if (recipe.Ingredients.Count == 1)
        {
            return new AssistantToolResult("A recipe needs at least one ingredient.");
        }

        var original = AssistantRecipes.ToProposed(recipe.Ingredients[arguments.Number - 1]);

        return AssistantProposal.Of(
            $"remove {AssistantRecipes.FormatIngredient(original)}",
            new RemoveIngredientAction(original)
        );
    }
}
