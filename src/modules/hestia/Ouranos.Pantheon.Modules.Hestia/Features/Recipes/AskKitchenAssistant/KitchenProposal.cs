using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;

internal static class KitchenProposal
{
    public static AssistantToolResult Of(string description, object action)
    {
        return new AssistantToolResult(
            $"Proposed to the user: {description}. It changes nothing until they apply it.",
            action
        );
    }
}
