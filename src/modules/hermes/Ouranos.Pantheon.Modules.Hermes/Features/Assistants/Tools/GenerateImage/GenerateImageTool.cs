using System.Globalization;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage;

public sealed class GenerateImageTool(
    ILogger<GenerateImageTool> logger,
    IOuranosMachineLearningClient mlClient,
    IDbContextFactory<HermesDbContext> dbContextFactory,
    IOptions<HermesOptions> options
) : AssistantTool<IConversationScope, GenerateImageArguments>(logger)
{
    public const string EmptyPromptOutput =
        "No image generated: the prompt was empty. Describe the image to draw.";

    public const string NoImageOutput = "Image generation finished without an image.";

    public const string ShownOutput = """
        The image is now shown to the user. Do not describe or mention it unless the user asks;
        if your reply is already complete, end it without adding text.
        """;

    private readonly ILogger<GenerateImageTool> _logger = Guard.Against.Null(logger);
    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);
    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory = Guard.Against.Null(
        dbContextFactory
    );
    private readonly HermesImageGenerationOptions _options = Guard
        .Against.Null(options)
        .Value.ImageGeneration;

    public override string Name => "generate_image";

    public override string Description =>
        """
            Draw the current scene or subject.
            To change an earlier image, call again with a full revised prompt.
            The image is shown to the user automatically; never add links or markdown images to your reply.
            """;

    public override bool IsSingleDeliveryPerReply => true;

    public override bool IsAvailable(IConversationScope context)
    {
        return _options.IsEnabled;
    }

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        IConversationScope context,
        GenerateImageArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var prompt = arguments.Prompt.Trim();
        if (prompt.Length == 0)
        {
            return new AssistantToolResult(EmptyPromptOutput, IsError: true);
        }

        if (prompt.Length > _options.MaxPromptLength)
        {
            prompt = prompt[.._options.MaxPromptLength];
        }

        var size = SizeFor(arguments.Orientation ?? ImageOrientation.Square);

        ImageGenerationCompletedDto? completed = null;
        var model = _options.Model ?? string.Empty;
        try
        {
            await foreach (
                var imageEvent in _mlClient.StreamImageGenerationAsync(
                    new GenerateImageRequest(
                        WithLoras(prompt, context.ImageLoras),
                        size.Width,
                        size.Height,
                        _options.Model
                    ),
                    cancellationToken
                )
            )
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
            completed.Seed
        );

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.GeneratedImages.AddAsync(image, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AssistantToolResult(
            ShownOutput,
            Image: new AssistantImage(
                image.Id.Value,
                $"/api/hermes/images/{image.Id.Value}",
                image.Width,
                image.Height,
                prompt
            )
        );
    }

    // SwarmUI applies LoRAs written into the prompt as <lora:name:weight>.
    private static string WithLoras(string prompt, IReadOnlyList<ImageLora> loras)
    {
        if (loras.Count == 0)
        {
            return prompt;
        }

        var tags = loras.Select(l =>
            string.Create(CultureInfo.InvariantCulture, $"<lora:{l.Identifier}:{l.Weight:0.##}>")
        );
        return $"{prompt} {string.Join(' ', tags)}";
    }

    private HermesImageSize SizeFor(ImageOrientation orientation)
    {
        return orientation switch
        {
            ImageOrientation.Portrait => _options.PortraitSize,
            ImageOrientation.Landscape => _options.LandscapeSize,
            _ => _options.SquareSize,
        };
    }
}
