using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;

public sealed class KitchenAssistant(
    ILogger<KitchenAssistant> logger,
    IOuranosMachineLearningClient mlClient,
    IHestiaMartenStore store,
    IOptions<HestiaOptions> options
) : PantheonAssistant<KitchenAssistantContext>(logger, mlClient)
{
    private readonly ILogger<KitchenAssistant> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);
    private readonly IOptions<HestiaOptions> _options = Guard.Against.Null(options);

    protected override async ValueTask<AssistantPrompt> BuildPromptAsync(
        KitchenAssistantContext context,
        CancellationToken cancellationToken
    )
    {
        _logger.LogTrace(
            "Attempting to build kitchen assistant prompt for recipe '{recipeId}'.",
            context.RecipeId
        );
        cancellationToken.ThrowIfCancellationRequested();

        if (!context.RecipeId.TryGetStreamId(out var streamId))
        {
            Guard.Against.NotFound(context.RecipeId, (Recipe?)null);
        }

        using var session = _store.QuerySession();
        var recipe = await session.LoadAsync<Recipe>(streamId, cancellationToken);
        Guard.Against.NotFound(context.RecipeId, recipe);

        var options = _options.Value.KitchenAssistant;

        _logger.LogDebug(
            "Successfully built kitchen assistant prompt for recipe '{recipeId}'.",
            context.RecipeId
        );
        return new AssistantPrompt(
            options.ModelName,
            KitchenAssistantPrompt.Compose(recipe),
            options.Temperature,
            options.MaxTokens,
            options.ReasoningEffort
        );
    }
}
