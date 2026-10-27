using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.SystemPrompt;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;

public sealed class GenerateCompletionHandler
    : IPantheonStreamHandler<GenerateCompletionInput, AssistantEvent>
{
    public const string ToolInstructions = "You can call tools. Never name tools in your reply.";

    private readonly ILogger<GenerateCompletionHandler> _logger;
    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory;
    private readonly ToolSetResolver _toolSetResolver;
    private readonly AssistantToolLoop<HermesChatContext> _loop;

    public GenerateCompletionHandler(
        ILogger<GenerateCompletionHandler> logger,
        IOuranosMachineLearningClient ouranosMachineLearningClient,
        IDbContextFactory<HermesDbContext> dbContextFactory,
        ToolSetResolver toolSetResolver
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(ouranosMachineLearningClient);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(toolSetResolver);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _toolSetResolver = toolSetResolver;
        _loop = new AssistantToolLoop<HermesChatContext>(logger, ouranosMachineLearningClient);
    }

    public async IAsyncEnumerable<AssistantEvent> Handle(
        GenerateCompletionInput command,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle generate completion query '{@query}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (command.ConversationId is not null)
        {
            await using var authContext = await _dbContextFactory.CreateDbContextAsync(
                cancellationToken
            );

            var conversation = await authContext
                .Conversations.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == command.ConversationId, cancellationToken);

            Guard.Against.NotFound(command.ConversationId.Value, conversation);
        }

        var model = command.Conversation.Model;

        var images = await LoadImagesAsync(command.Conversation.Messages, cancellationToken);
        var loras = await LoadImageLorasAsync(command.Conversation.LoraIds, cancellationToken);

        var context = new HermesChatContext(command.ConversationId, loras, images);

        var toolSet = await _toolSetResolver.ResolveAsync(
            command.Conversation.ToolSetId,
            context,
            cancellationToken
        );

        var systemPrompt = ComposeSystemPrompt(command.Conversation);
        var instructions = ComposeInstructions(systemPrompt, toolSet);

        yield return new AssistantInstructionsEvent(instructions);

        var prompt = new AssistantPrompt(
            model.ModelIdentifier,
            systemPrompt,
            model.Temperature,
            model.MaxTokens ?? ModelDefaults.MaxTokens,
            command.Conversation.ReasoningEffort
        );

        var reply = new MessagePartsBuilder();
        AssistantUsageEvent? usage = null;

        await foreach (
            var assistantEvent in _loop.RunAsync(
                prompt,
                instructions,
                BuildInput(command.Conversation.Messages, images, prompt.MaxToolOutputCharacters),
                toolSet.Tools,
                context,
                cancellationToken
            )
        )
        {
            yield return assistantEvent;

            if (assistantEvent is AssistantErrorEvent)
            {
                yield break;
            }

            if (assistantEvent is AssistantUsageEvent usageEvent)
            {
                usage = usageEvent;
            }

            reply.Apply(assistantEvent);
        }

        if (command.ConversationId is not null)
        {
            await PersistTurnAsync(
                command.ConversationId.Value,
                command.Conversation.Messages,
                reply,
                usage,
                cancellationToken
            );
        }

        yield return new AssistantDoneEvent();

        _logger.LogDebug("Successfully handled generate completion request.");
    }

    internal static string ComposeSystemPrompt(ConversationInput conversation)
    {
        var builder = new SystemPromptBuilder()
            .WithPersona(
                conversation.Persona.Name,
                conversation.Persona.Description,
                conversation.Persona.Personality,
                conversation.Persona.Scenario
            )
            .WithModel(conversation.Model.SystemPrompt);

        if (conversation.Traits is not null)
        {
            foreach (var trait in conversation.Traits)
            {
                builder.AddTrait(trait.Name, trait.Content);
            }
        }

        return builder.Build();
    }

    internal static string ComposeInstructions(string systemPrompt, ResolvedToolSet toolSet)
    {
        if (toolSet.Tools.Count == 0)
        {
            return systemPrompt;
        }

        return string.IsNullOrWhiteSpace(toolSet.Instructions)
            ? $"{systemPrompt}\n\n{ToolInstructions}"
            : $"{systemPrompt}\n\n{ToolInstructions}\n{toolSet.Instructions}";
    }

    // Earlier tool calls are replayed before the reply that made them so the model can build on
    // what its tools returned; reasoning is not replayed. Images are replayed as their short refs,
    // with the prompt of an image the user attached so the model can revise it.
    internal static List<ResponseInputItemDto> BuildInput(
        IEnumerable<CompletionMessageInput> messages,
        ConversationImages images,
        int maxToolOutputCharacters
    )
    {
        var input = new List<ResponseInputItemDto>();

        foreach (var message in messages)
        {
            if (message.Role == Role.Assistant)
            {
                foreach (var call in message.ToolCalls ?? [])
                {
                    var output = call.Output.TruncateWithEllipsis(maxToolOutputCharacters);
                    if (call.ImageId is { } imageId && images.Find(imageId) is { } image)
                    {
                        output = $"{output} It is image {image.Ref}.";
                    }

                    input.Add(
                        new FunctionCallInputItemDto(
                            new FunctionCallDto(call.CallId, call.Name, call.Arguments)
                        )
                    );
                    input.Add(new FunctionCallOutputInputItemDto(call.CallId, output));
                }
            }

            var attachments = (message.Images ?? [])
                .Select(i => images.Find(i.ImageId))
                .OfType<ConversationImage>()
                .Select(i => $"[Attached image {i.Ref}: \"{i.Prompt}\"]");
            var content = string.Join('\n', [.. attachments, message.Content]).Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            input.Add(new MessageInputItemDto(new MessageDto(content, message.Role.ToRoleDto())));
        }

        return input;
    }

    // Numbers each image by where it first appears in the history; images that no longer exist
    // get no number.
    // A LoRA gone from the image server is skipped: OuranosMl rejects unknown LoRAs, which would
    // fail the whole image, and the UI already shows it as unavailable.
    private async Task<List<ImageLora>> LoadImageLorasAsync(
        List<Id<Lora>>? loraIds,
        CancellationToken cancellationToken
    )
    {
        if (loraIds is not { Count: > 0 })
        {
            return [];
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext
            .Loras.AsNoTracking()
            .Where(l =>
                loraIds.Contains(l.Id)
                && dbContext.AvailableLoras.Any(a => a.Identifier == l.Identifier)
            )
            .Select(l => new ImageLora(l.Identifier, l.Weight, l.TriggerPhrase))
            .ToListAsync(cancellationToken);
    }

    private async Task<ConversationImages> LoadImagesAsync(
        IReadOnlyList<CompletionMessageInput> messages,
        CancellationToken cancellationToken
    )
    {
        var referenced = messages.SelectMany(ImageIdsOf).ToList();
        if (referenced.Count == 0)
        {
            return ConversationImages.Empty;
        }

        var ids = referenced.Distinct().ToList();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var prompts = await dbContext
            .GeneratedImages.AsNoTracking()
            .Where(i => ids.Contains(i.Id))
            .Select(i => new { i.Id, i.Prompt })
            .ToDictionaryAsync(i => i.Id, i => i.Prompt, cancellationToken);

        return ToConversationImages(messages, prompts);
    }

    internal static ConversationImages ToConversationImages(
        IReadOnlyList<CompletionMessageInput> messages,
        IReadOnlyDictionary<Id<GeneratedImage>, string> prompts
    )
    {
        var images = new Dictionary<Id<GeneratedImage>, ConversationImage>();
        ConversationImage? latest = null;
        foreach (var id in messages.SelectMany(ImageIdsOf))
        {
            if (!prompts.TryGetValue(id, out var prompt))
            {
                continue;
            }

            if (!images.TryGetValue(id, out var image))
            {
                image = new ConversationImage(images.Count + 1, id, prompt);
                images.Add(id, image);
            }

            latest = image;
        }

        var last = messages.Count > 0 ? messages[^1] : null;
        var attached =
            last?.Role == Role.User
                ? (last.Images ?? [])
                    .Select(i => images.GetValueOrDefault(i.ImageId))
                    .FirstOrDefault(i => i is not null)
                : null;

        return new ConversationImages([.. images.Values], attached, latest);
    }

    private static IEnumerable<Id<GeneratedImage>> ImageIdsOf(CompletionMessageInput message)
    {
        foreach (var call in message.ToolCalls ?? [])
        {
            if (call.ImageId is { } imageId)
            {
                yield return imageId;
            }
        }

        foreach (var image in message.Images ?? [])
        {
            yield return image.ImageId;
        }
    }

    private async Task PersistTurnAsync(
        Id<Conversation> conversationId,
        List<CompletionMessageInput> messages,
        MessagePartsBuilder reply,
        AssistantUsageEvent? usage,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var conversation = await dbContext.Conversations.FirstOrDefaultAsync(
            c => c.Id == conversationId,
            cancellationToken
        );

        Guard.Against.NotFound(conversationId, conversation);

        var existingCount = await dbContext.Messages.CountAsync(
            m => m.ConversationId == conversationId,
            cancellationToken
        );

        var userInput = messages.LastOrDefault(m => m.Role == Role.User);
        if (userInput is not null)
        {
            await dbContext.Messages.AddAsync(
                Message.Create(
                    DatabaseExtensions.CreateId<Message>(),
                    conversationId,
                    userInput.Content,
                    Role.User,
                    existingCount,
                    [.. (userInput.Images ?? []).Select(MessagePartMapping.ToEntity)]
                ),
                cancellationToken
            );
        }

        await dbContext.Messages.AddAsync(
            Message.Create(
                DatabaseExtensions.CreateId<Message>(),
                conversationId,
                reply.Content,
                Role.Assistant,
                existingCount + 1,
                reply.Build()
            ),
            cancellationToken
        );

        // The total tracks how much of the context window the conversation fills, which after a
        // tool round is the last round's size rather than the sum over rounds.
        if (usage is not null)
        {
            conversation.RecordTokenUsage(
                usage.InputTokens,
                usage.OutputTokens,
                usage.ContextTokens
            );
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

}
