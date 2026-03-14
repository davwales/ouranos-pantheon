using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;
using Ouranos.Pantheon.Hermes.Service.Application.Commands.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Hermes.Service.Domain.Conversations;

namespace Ouranos.Pantheon.Hermes.Service.Infra.OuranosMl.Conversations;

public sealed class GenerateChatCompletion : IGenerateChatCompletion
{
    private readonly ILogger<GenerateChatCompletion> _logger;
    private readonly IOuranosMachineLearningClient _ouranosClient;

    public GenerateChatCompletion(
        ILogger<GenerateChatCompletion> logger,
        IOuranosMachineLearningClient ouranosClient
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(ouranosClient);

        _logger = logger;
        _ouranosClient = ouranosClient;
    }

    public async IAsyncEnumerable<string> GenerateCompletionStream(
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

        await foreach (var line in _ouranosClient.GenerateChatCompletion(request, cancellationToken))
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