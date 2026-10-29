using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.EditImage.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.EditImage;

/// <summary>
/// Redraws an earlier image from a revised prompt (img2img), keeping its composition. The source
/// is the image the user attached, else the one the model names, else the latest.
/// </summary>
public sealed partial class EditImageTool(
    ILogger<EditImageTool> logger,
    IOuranosMachineLearningClient mlClient,
    IDbContextFactory<HermesDbContext> dbContextFactory,
    IOptions<HermesOptions> options
) : ImageTool<EditImageArguments>(logger, dbContextFactory, options)
{
    public const string ToolName = "edit_image";

    public const string NoSourceOutput =
        $"No image edited: there is no image to edit yet. Use {GenerateImageTool.ToolName} to draw one.";

    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);

    public override string Name => ToolName;

    public override string Description =>
        """
            Change an earlier image while keeping its composition.
            Use it when the user attaches an image or asks to change, vary or redo an image, or wants more like it.
            For "more like this", reuse the image's prompt unchanged with change: subtle.
            The image is shown to the user automatically; never add links or markdown images to your reply.
            """;

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        IConversationScope context,
        EditImageArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var prompt = CapPrompt(arguments.Prompt);
        if (prompt is null)
        {
            return new AssistantToolResult(EmptyPromptOutput, IsError: true);
        }

        var images = context.Images;
        var source = images.Attached;
        if (source is null && !string.IsNullOrWhiteSpace(arguments.Image))
        {
            var match = RefPattern().Match(arguments.Image);
            source = int.TryParse(match.Value, out var imageRef)
                ? images.FindByRef(imageRef)
                : null;
            if (source is null)
            {
                return new AssistantToolResult(
                    UnknownRefOutput(arguments.Image, images),
                    IsError: true
                );
            }
        }

        source ??= images.Latest;
        if (source is null)
        {
            return new AssistantToolResult(NoSourceOutput, IsError: true);
        }

        await using var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var content = await dbContext
            .GeneratedImages.AsNoTracking()
            .Where(i => i.Id == source.Id)
            .Select(i => i.Content)
            .FirstOrDefaultAsync(cancellationToken);

        if (content is null)
        {
            return new AssistantToolResult(
                $"No image edited: image {source.Ref} is no longer available.",
                IsError: true
            );
        }

        return await DeliverAsync(
            _mlClient.StreamImageEditAsync(
                new EditImageRequest(
                    WithTriggerPhrases(prompt, context.ImageLoras),
                    content,
                    StrengthFor(arguments.Change),
                    Options.Model,
                    ToLoraRequests(context.ImageLoras)
                ),
                cancellationToken
            ),
            prompt,
            $"Edited image {source.Ref}. {ShownOutput}",
            source.Id,
            cancellationToken
        );
    }

    private static string UnknownRefOutput(string imageRef, ConversationImages images)
    {
        var known =
            images.Images.Count == 0
                ? "there are no images yet"
                : $"use one of {string.Join(", ", images.Images.Select(i => $"image {i.Ref}"))}";
        return $"No image edited: \"{imageRef}\" is not an image in this chat; {known}.";
    }

    private float StrengthFor(ImageChange change)
    {
        var strengths = Options.EditStrengths;
        var strength = change switch
        {
            ImageChange.Subtle => strengths.Subtle,
            ImageChange.Strong => strengths.Strong,
            _ => strengths.Moderate,
        };
        return Math.Clamp(strength, 0f, 1f);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex RefPattern();
}
