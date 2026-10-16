using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.AddIngredient.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.AddIngredient;

public sealed class AddIngredientTool(ILogger<AddIngredientTool> logger)
    : AssistantTool<IRecipeScope, AddIngredientArguments>(logger)
{
    public override string Name => "add_ingredient";

    public override string Description =>
        "Propose adding an ingredient. Leave the quantity null for 'to taste', use the unit "
        + $"'{AssistantRecipes.UnitlessUnit}' for countable items, and mark optional ingredients.";

    protected override ValueTask<AssistantToolResult> ExecuteAsync(
        IRecipeScope context,
        AddIngredientArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var ingredient = AssistantRecipes.Propose(
            arguments.Quantity,
            arguments.Unit,
            arguments.Name,
            arguments.Optional
        );

        if (AssistantRecipes.Validate(ingredient) is { } problem)
        {
            return ValueTask.FromResult(new AssistantToolResult(problem));
        }

        return ValueTask.FromResult(
            AssistantProposal.Of(
                $"add {AssistantRecipes.FormatIngredient(ingredient)}",
                new AddIngredientAction(ingredient)
            )
        );
    }
}
