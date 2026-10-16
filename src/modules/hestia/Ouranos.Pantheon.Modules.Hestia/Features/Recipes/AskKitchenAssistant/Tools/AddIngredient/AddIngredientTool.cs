using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddIngredient.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddIngredient;

public sealed class AddIngredientTool(ILogger<AddIngredientTool> logger)
    : AssistantTool<KitchenAssistantContext, AddIngredientArguments>(logger)
{
    public override string Name => "add_ingredient";

    public override string Description =>
        "Propose adding an ingredient. Leave the quantity null for 'to taste', use the unit "
        + $"'{KitchenRecipes.UnitlessUnit}' for countable items, and mark optional ingredients.";

    protected override ValueTask<AssistantToolResult> ExecuteAsync(
        KitchenAssistantContext context,
        AddIngredientArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var ingredient = KitchenRecipes.Propose(
            arguments.Quantity,
            arguments.Unit,
            arguments.Name,
            arguments.Optional
        );

        if (KitchenRecipes.Validate(ingredient) is { } problem)
        {
            return ValueTask.FromResult(new AssistantToolResult(problem));
        }

        return ValueTask.FromResult(
            KitchenProposal.Of(
                $"add {KitchenRecipes.FormatIngredient(ingredient)}",
                new AddIngredientAction(ingredient)
            )
        );
    }
}
