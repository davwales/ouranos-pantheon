using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;

public sealed class RecipeExtractor(
    ILogger<RecipeExtractor> logger,
    IOuranosMachineLearningClient mlClient,
    IOptions<HestiaOptions> options
) : IRecipeExtractor
{
    private const int MaxJsonLdLength = 100_000;

    public const string SystemPrompt = """
        You are a recipe extraction assistant. You receive a schema.org Recipe JSON-LD object
        extracted from a web page and convert it into a clean, structured recipe for storage.

        Follow these rules exactly:

        - title: The recipe name, trimmed.
        - description: The recipe's summary or introduction. Use null when absent or meaningless.
        - ingredients: One entry per distinct ingredient. Parse each source line into:
          - quantity: the numeric amount as a decimal. Convert fractions and unicode numerals
            ("1/2" -> 0.5, "½" -> 0.5, "2-3" -> 3, "one" -> 1). Use null when the line has no amount
            (for example "to taste" or "a pinch").
          - unit: the measurement unit in lowercase singular form (cup, tbsp, tsp, g, kg, ml, oz,
            clove, slice, pinch). Use null for countable or unitless items, such as "3 eggs".
          - name: the ingredient name only, without the amount, unit, or trailing preparation notes.
          - When one line lists multiple distinct ingredients (for example "2 cups flour, plus more
            for dusting"), emit separate entries per ingredient and keep preparation context only
            on the ingredient it describes.
        - steps: The ordered instruction steps as self-contained strings. Flatten HowToSection
          groupings into their child steps. Do not number the steps and do not include numbering
          prefixes such as "1." or "Step 1:".
        - Preserve the original units and casing of ingredient names, but normalize unit spelling
          and casing as described above.

        Respond with a single JSON object matching the requested schema. No markdown code fences,
        no commentary, no trailing text.
        """;

    private readonly ILogger<RecipeExtractor> _logger = Guard.Against.Null(logger);
    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);
    private readonly IOptions<HestiaOptions> _options = Guard.Against.Null(options);

    public async Task<ExtractedRecipe?> ExtractAsync(
        string recipeJsonLd,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to extract recipe from JSON-LD.");
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(recipeJsonLd))
        {
            return null;
        }

        if (recipeJsonLd.Length > MaxJsonLdLength)
        {
            _logger.LogWarning(
                "Recipe extraction skipped: JSON-LD exceeds the maximum allowed size of {maxLength} characters.",
                MaxJsonLdLength
            );
            return null;
        }

        var model = _options.Value.RecipeImport.ModelName;
        if (string.IsNullOrWhiteSpace(model))
        {
            _logger.LogWarning(
                "Recipe extraction skipped: no recipe import model is configured (Ouranos:Hestia:RecipeImport:ModelName)."
            );
            return null;
        }

        List<MessageDto> messages =
        [
            new(SystemPrompt, RoleDto.System),
            new(recipeJsonLd, RoleDto.User),
        ];

        var result = await _mlClient.GenerateStructuredChatCompletionAsync<ExtractedRecipe>(
            model,
            messages,
            _options.Value.RecipeImport.Temperature,
            _options.Value.RecipeImport.MaxTokens,
            cancellationToken
        );

        if (result is null)
        {
            _logger.LogWarning("Recipe extraction produced an empty or unparseable completion.");
            return null;
        }

        if (
            string.IsNullOrWhiteSpace(result.Title)
            || result.Ingredients.Count == 0
            || result.Steps.Count == 0
        )
        {
            _logger.LogWarning("Recipe extraction produced a completion missing required fields.");
            return null;
        }

        _logger.LogDebug(
            "Successfully extracted recipe '{title}' with {ingredientCount} ingredients and {stepCount} steps.",
            result.Title,
            result.Ingredients.Count,
            result.Steps.Count
        );
        return result;
    }
}
