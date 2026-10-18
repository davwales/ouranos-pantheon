using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
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
    public const string ToolInstructions = """
        You can call tools. Call one whenever it helps the reply, without asking the user first,
        and never name tools in your reply.
        """;

    private readonly ILogger<GenerateCompletionHandler> _logger;
    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory;
    private readonly IReadOnlyList<IAssistantTool<HermesChatContext>> _tools;
    private readonly AssistantToolLoop<HermesChatContext> _loop;

    public GenerateCompletionHandler(
        ILogger<GenerateCompletionHandler> logger,
        IOuranosMachineLearningClient ouranosMachineLearningClient,
        IDbContextFactory<HermesDbContext> dbContextFactory,
        IEnumerable<IAssistantTool<HermesChatContext>> tools
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(ouranosMachineLearningClient);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(tools);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _tools = [.. tools];
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
        var context = new HermesChatContext(
            command.ConversationId,
            [
                .. (command.Conversation.Loras ?? []).Select(l => new ImageLora(
                    l.Identifier,
                    l.Weight
                )),
            ]
        );
        List<IAssistantTool<HermesChatContext>> tools = model.IsToolCallingEnabled
            ? [.. _tools.Where(t => t.IsAvailable(context))]
            : [];

        var systemPrompt = ComposeSystemPrompt(command.Conversation);
        var instructions = tools.Count > 0 ? $"{systemPrompt}\n\n{ToolInstructions}" : systemPrompt;

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
                BuildInput(command.Conversation.Messages, prompt.MaxToolOutputCharacters),
                tools,
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

    // Earlier tool calls are replayed before the reply that made them so the model can build on
    // what its tools returned; reasoning is not replayed.
    internal static List<ResponseInputItemDto> BuildInput(
        IEnumerable<CompletionMessageInput> messages,
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
                    input.Add(
                        new FunctionCallInputItemDto(
                            new FunctionCallDto(call.CallId, call.Name, call.Arguments)
                        )
                    );
                    input.Add(
                        new FunctionCallOutputInputItemDto(
                            call.CallId,
                            Truncate(call.Output, maxToolOutputCharacters)
                        )
                    );
                }
            }

            if (string.IsNullOrWhiteSpace(message.Content))
            {
                continue;
            }

            input.Add(
                new MessageInputItemDto(new MessageDto(message.Content, MapRole(message.Role)))
            );
        }

        return input;
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
                    existingCount
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

    private static string Truncate(string output, int maxCharacters)
    {
        return output.Length <= maxCharacters ? output : $"{output[..maxCharacters]}…";
    }

    private static RoleDto MapRole(Role role)
    {
        return role switch
        {
            Role.System => RoleDto.System,
            Role.User => RoleDto.User,
            Role.Assistant => RoleDto.Assistant,
            Role.Summary => RoleDto.Assistant,
            _ => throw new InvalidOperationException($"Unknown role: {role}"),
        };
    }
}
