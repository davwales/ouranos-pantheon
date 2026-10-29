using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools;

/// <summary>
/// Base class for the tools that draw an image with OuranosMl, store it and show it to the user.
/// </summary>
public abstract class ImageTool<TArguments>(
    ILogger logger,
    IDbContextFactory<HermesDbContext> dbContextFactory,
    IOptions<HermesOptions> options
) : AssistantTool<IConversationScope, TArguments>(logger)
    where TArguments : class
{
    public const string EmptyPromptOutput =
        "No image generated: the prompt was empty. Describe the image to draw.";

    public const string NoImageOutput = "Image generation finished without an image.";

    public const string ShownOutput = """
        The image is now shown to the user. Do not describe or mention it unless the user asks;
        if your reply is already complete, end it without adding text.
        """;

    private readonly ILogger _logger = Guard.Against.Null(logger);

    protected IDbContextFactory<HermesDbContext> DbContextFactory { get; } =
        Guard.Against.Null(dbContextFactory);

    protected HermesImageGenerationOptions Options { get; } =
        Guard.Against.Null(options).Value.ImageGeneration;

    public override bool IsSingleDeliveryPerReply => true;

    public override bool IsAvailable(IConversationScope context)
    {
        return Options.IsEnabled;
    }

    /// <summary>
    /// The trimmed prompt capped at the configured length, or null when it is blank.
    /// </summary>
    protected string? CapPrompt(string prompt)
    {
        prompt = prompt.Trim();
        if (prompt.Length == 0)
        {
            return null;
        }

        return prompt.Length > Options.MaxPromptLength ? prompt[..Options.MaxPromptLength] : prompt;
    }

    /// <summary>
    /// Consumes an OuranosMl image stream, then stores and returns the image it produced.
    /// </summary>
    protected async Task<AssistantToolResult> DeliverAsync(
        IAsyncEnumerable<ImageGenerationEventDto> events,
        string prompt,
        string shownOutput,
        Id<GeneratedImage>? sourceImageId,
        CancellationToken cancellationToken
    )
    {
        ImageGenerationCompletedDto? completed = null;
        var model = Options.Model ?? string.Empty;
        try
        {
            await foreach (var imageEvent in events.WithCancellation(cancellationToken))
            {
                switch (imageEvent)
                {
                    case ImageGenerationCompletedDto generated:
                        completed = generated;
                        break;
                    case ImageGenerationDoneDto done:
                        model = done.Model;
                        break;
                }
            }
        }
        catch (OuranosMlRequestException ex)
        {
            _logger.LogWarning(ex, "Image generation failed with '{type}'.", ex.Type);
            return new AssistantToolResult($"No image generated: {ex.Message}", IsError: true);
        }

        if (completed is null)
        {
            return new AssistantToolResult(NoImageOutput, IsError: true);
        }

        var image = GeneratedImage.Create(
            DatabaseExtensions.CreateId<GeneratedImage>(),
            completed.Content,
            completed.MediaType,
            completed.Width,
            completed.Height,
            prompt,
            model,
            completed.Seed,
            sourceImageId
        );

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.GeneratedImages.AddAsync(image, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AssistantToolResult(
            shownOutput,
            Image: new AssistantImage(
                image.Id.Value,
                $"/api/hermes/images/{image.Id.Value}",
                image.Width,
                image.Height,
                prompt
            )
        );
    }

    // A LoRA only takes effect when its trigger phrase is in the prompt; it goes first, as LoRA
    // authors write it. The stored prompt stays the model's own, so edits and "more like this"
    // don't repeat the phrase.
    protected static string WithTriggerPhrases(string prompt, IReadOnlyList<ImageLora> loras)
    {
        var phrases = loras
            .Select(l => l.TriggerPhrase)
            .OfType<string>()
            .Where(p => !prompt.Contains(p, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return phrases.Count == 0 ? prompt : $"{string.Join(", ", phrases)}, {prompt}";
    }

    protected static List<ImageLoraRequest> ToLoraRequests(IReadOnlyList<ImageLora> loras)
    {
        return [.. loras.Select(l => new ImageLoraRequest(l.Identifier, l.Weight))];
    }
}
