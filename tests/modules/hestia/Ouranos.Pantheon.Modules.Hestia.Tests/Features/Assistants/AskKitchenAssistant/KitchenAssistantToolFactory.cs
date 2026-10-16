using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.AddIngredient;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.AddNote;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.RemoveIngredient;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.UpdateIngredient;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.UpdateStep;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Assistants.AskKitchenAssistant;

public static class KitchenAssistantToolFactory
{
    public static IReadOnlyList<IAssistantTool<KitchenAssistantContext>> All(
        IHestiaMartenStore? store = null
    )
    {
        store ??= Substitute.For<IHestiaMartenStore>();

        return
        [
            new AddNoteTool(Substitute.For<ILogger<AddNoteTool>>(), store),
            new AddIngredientTool(Substitute.For<ILogger<AddIngredientTool>>()),
            new UpdateIngredientTool(Substitute.For<ILogger<UpdateIngredientTool>>(), store),
            new RemoveIngredientTool(Substitute.For<ILogger<RemoveIngredientTool>>(), store),
            new UpdateStepTool(Substitute.For<ILogger<UpdateStepTool>>(), store),
        ];
    }
}
