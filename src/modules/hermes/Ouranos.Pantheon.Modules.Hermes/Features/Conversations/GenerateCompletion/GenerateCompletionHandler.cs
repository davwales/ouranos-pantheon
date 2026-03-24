using System.Runtime.CompilerServices;
using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;

public sealed class GenerateCompletionHandler
    : IPantheonHandler<GenerateCompletionInput, StreamResponse<string, GenerateCompletionResponse>>
{
    private readonly ILogger<GenerateCompletionHandler> _logger;
    private readonly IOuranosMachineLearningClient _ouranosMachineLearningClient;

    public GenerateCompletionHandler(
        ILogger<GenerateCompletionHandler> logger,
        IOuranosMachineLearningClient ouranosMachineLearningClient
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(ouranosMachineLearningClient);

        _logger = logger;
        _ouranosMachineLearningClient = ouranosMachineLearningClient;
    }

    public async Task<StreamResponse<string, GenerateCompletionResponse>> Handle(
        GenerateCompletionInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle generate completion query '{@query}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var stream = new StreamResponse<string, GenerateCompletionResponse>(
            async token => await Task.FromResult(GenerateCompletionStream(command.Conversation, token)),
            async chunk => await Task.FromResult(new GenerateCompletionResponse(chunk))
        );

        _logger.LogDebug("Successfully handled generate completion request.");
        return await Task.FromResult(stream);
    }

    private async IAsyncEnumerable<string> GenerateCompletionStream(
        ConversationInput conversation,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to generate a chat completion for conversation '{@conversation}'.", conversation);
        cancellationToken.ThrowIfCancellationRequested();

        var systemPrompt = ComposeSystemPrompt(
            conversation.Persona,
            conversation.Model.SystemPrompt,
            conversation.Traits
        );

        var request = new GenerateChatCompletionRequest(
            conversation.Model.ModelIdentifier,
            [
                new MessageDto(systemPrompt, MapRole(Role.System)),
                .. conversation.Messages.Select(m => new MessageDto(
                        m.Content,
                        MapRole(m.Role)
                    )
                )
            ],
            conversation.Model.Temperature,
            conversation.Model.MaxTokens,
            conversation.Model.RepeatPenalty
        );

        await foreach (var line in _ouranosMachineLearningClient.GenerateChatCompletion(request, cancellationToken))
        {
            yield return line;
            cancellationToken.ThrowIfCancellationRequested();
        }

        _logger.LogDebug("Successfully generated a chat completion.");
    }

    private static string ComposeSystemPrompt(PersonaInput persona, string systemPrompt, List<TraitInput>? traits)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{persona.Name}: {persona.Description}");

        if (!string.IsNullOrWhiteSpace(persona.Personality))
        {
            builder.AppendLine($"Personality: {persona.Personality}");
        }

        if (!string.IsNullOrWhiteSpace(persona.Scenario))
        {
            builder.AppendLine($"Scenario: {persona.Scenario}");
        }

        builder.AppendLine();
        builder.Append(systemPrompt);

        if (traits is { Count: > 0 })
        {
            foreach (var trait in traits)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine($"[{trait.Name}]");
                builder.Append(trait.Content);
            }
        }

        return builder.ToString();
    }

    private static RoleDto MapRole(Role role)
    {
        return role switch
        {
            Role.System => RoleDto.System,
            Role.User => RoleDto.User,
            Role.Assistant => RoleDto.Assistant,
            _ => throw new InvalidOperationException($"Unknown role: {role}")
        };
    }
}
