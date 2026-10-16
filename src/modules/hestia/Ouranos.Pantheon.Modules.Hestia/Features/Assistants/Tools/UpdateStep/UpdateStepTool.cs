using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.UpdateStep.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.UpdateStep;

public sealed class UpdateStepTool(ILogger<UpdateStepTool> logger, IHestiaMartenStore store)
    : AssistantTool<IRecipeScope, UpdateStepArguments>(logger)
{
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public override string Name => "update_step";

    public override string Description => "Propose rewording step number N with the full new text.";

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        IRecipeScope context,
        UpdateStepArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var text = arguments.Text.Trim();
        if (AssistantRecipes.ValidateStep(text) is { } problem)
        {
            return new AssistantToolResult(problem);
        }

        var recipe = await AssistantRecipes.LoadAsync(_store, context, cancellationToken);
        if (
            AssistantRecipes.NumberProblem(arguments.Number, recipe.Steps.Count, "step") is
            { } numberProblem
        )
        {
            return new AssistantToolResult(numberProblem);
        }

        var original = recipe.Steps[arguments.Number - 1].Text;

        return AssistantProposal.Of(
            $"reword step {arguments.Number} to \"{text}\"",
            new UpdateStepAction(original, text)
        );
    }
}
