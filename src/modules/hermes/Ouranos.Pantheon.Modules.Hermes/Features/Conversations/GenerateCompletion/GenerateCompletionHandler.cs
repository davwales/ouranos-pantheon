using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;

public sealed class GenerateCompletionHandler
    : CommandHandler<GenerateCompletionInput, StreamResponse<string, GenerateCompletionResponse>>
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

    public override async Task<StreamResponse<string, GenerateCompletionResponse>> Handle(
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

        var request = new GenerateChatCompletionRequest(
            conversation.Assistant.Model,
            [
                new MessageDto(conversation.Assistant.SystemPrompt, MapRole(Role.System)),
                .. conversation.Messages.Select(m => new MessageDto(
                        m.Content,
                        MapRole(m.Role)
                    )
                )
            ],
            conversation.Assistant.Temperature,
            conversation.Assistant.MaxTokens,
            conversation.Assistant.RepeatPenalty
        );

        await foreach (var line in _ouranosMachineLearningClient.GenerateChatCompletion(request, cancellationToken))
        {
            yield return line;
            cancellationToken.ThrowIfCancellationRequested();
        }

        _logger.LogDebug("Successfully generated a chat completion.");
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