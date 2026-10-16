using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddNote.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddNote;

public sealed class AddNoteTool(ILogger<AddNoteTool> logger, IHestiaMartenStore store)
    : AssistantTool<KitchenAssistantContext, AddNoteArguments>(logger)
{
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public override string Name => "add_note";

    public override string Description =>
        "Propose a note to append to the recipe's notes, such as a tip, substitution or storage advice.";

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        KitchenAssistantContext context,
        AddNoteArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var text = arguments.Text.Trim();
        if (text.Length == 0)
        {
            return new AssistantToolResult("The note is empty.");
        }

        var recipe = await KitchenRecipes.LoadAsync(_store, context, cancellationToken);
        if (recipe.Notes.Length + text.Length + 1 > Recipe.MaxNotesLength)
        {
            return new AssistantToolResult("The notes would be too long; propose a shorter note.");
        }

        return KitchenProposal.Of($"add the note \"{text}\"", new AddRecipeNoteAction(text));
    }
}
