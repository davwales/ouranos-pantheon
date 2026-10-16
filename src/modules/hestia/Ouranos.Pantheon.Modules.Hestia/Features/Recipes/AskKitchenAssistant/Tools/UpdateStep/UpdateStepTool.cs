using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateStep.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateStep;

public sealed class UpdateStepTool(ILogger<UpdateStepTool> logger, IHestiaMartenStore store)
    : AssistantTool<KitchenAssistantContext, UpdateStepArguments>(logger)
{
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public override string Name => "update_step";

    public override string Description => "Propose rewording step number N with the full new text.";

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        KitchenAssistantContext context,
        UpdateStepArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var text = arguments.Text.Trim();
        if (KitchenRecipes.ValidateStep(text) is { } problem)
        {
            return new AssistantToolResult(problem);
        }

        var recipe = await KitchenRecipes.LoadAsync(_store, context, cancellationToken);
        if (
            KitchenRecipes.NumberProblem(arguments.Number, recipe.Steps.Count, "step") is
            { } numberProblem
        )
        {
            return new AssistantToolResult(numberProblem);
        }

        var original = recipe.Steps[arguments.Number - 1].Text;

        return KitchenProposal.Of(
            $"reword step {arguments.Number} to \"{text}\"",
            new UpdateStepAction(original, text)
        );
    }
}
