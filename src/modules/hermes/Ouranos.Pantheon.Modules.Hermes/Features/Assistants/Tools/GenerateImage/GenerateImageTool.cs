using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage;

public sealed class GenerateImageTool(
    ILogger<GenerateImageTool> logger,
    IOuranosMachineLearningClient mlClient,
    IDbContextFactory<HermesDbContext> dbContextFactory,
    IOptions<HermesOptions> options
) : ImageTool<GenerateImageArguments>(logger, dbContextFactory, options)
{
    public const string ToolName = "generate_image";

    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);

    public override string Name => ToolName;

    public override string Description =>
        """
            Draw a new image of the current scene or subject from scratch.
            To change an earlier image, use edit_image if you have it; otherwise call again with a full revised prompt.
            The image is shown to the user automatically; never add links or markdown images to your reply.
            """;

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        IConversationScope context,
        GenerateImageArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var prompt = CapPrompt(arguments.Prompt);
        if (prompt is null)
        {
            return new AssistantToolResult(EmptyPromptOutput, IsError: true);
        }

        var size = SizeFor(arguments.Orientation ?? ImageOrientation.Square);

        return await DeliverAsync(
            _mlClient.StreamImageGenerationAsync(
                new GenerateImageRequest(
                    WithLoras(prompt, context.ImageLoras),
                    size.Width,
                    size.Height,
                    Options.Model
                ),
                cancellationToken
            ),
            prompt,
            ShownOutput,
            sourceImageId: null,
            cancellationToken
        );
    }

    private HermesImageSize SizeFor(ImageOrientation orientation)
    {
        return orientation switch
        {
            ImageOrientation.Portrait => Options.PortraitSize,
            ImageOrientation.Landscape => Options.LandscapeSize,
            _ => Options.SquareSize,
        };
    }
}
