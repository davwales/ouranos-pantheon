using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;
using Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl.Conversations;

public sealed class GenerateCompletion : IGenerateCompletion
{
    private readonly ILogger<GenerateCompletion> _logger;
    private readonly IOuranosMachineLearningClient _ouranosClient;
    private readonly string _systemPrompt;

    public GenerateCompletion(
        ILogger<GenerateCompletion> logger,
        IOuranosMachineLearningClient ouranosClient,
        IOptions<OuranosMachineLearningOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(ouranosClient);
        Guard.Against.Null(options);
        Guard.Against.Null(options.Value);

        _logger = logger;
        _ouranosClient = ouranosClient;
        _systemPrompt = options.Value.SystemPrompt;
    }

    public async IAsyncEnumerable<string> GenerateCompletionStream(
        ConversationInput conversation,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to generate a completion for conversation '{@conversation}'.", conversation);
        cancellationToken.ThrowIfCancellationRequested();

        var request = GetRequest(conversation);
        await foreach (var line in _ouranosClient.GenerateCompletion(request, cancellationToken))
        {
            yield return line;
            cancellationToken.ThrowIfCancellationRequested();
        }

        _logger.LogDebug("Successfully generated a completion.");
    }

    private GenerateCompletionRequest GetRequest(ConversationInput conversation)
    {
        var conversationVariables = GetConversationVariables(conversation);

        var cleanedSystemPrompt = CleanContent(_systemPrompt, conversationVariables);
        var cleanedContext = CleanContent(conversation.Context, conversationVariables);

        var cleanedMessages = conversation.Messages
            .Select(m =>
            {
                var role = m.Role switch
                {
                    Role.System => RoleDto.System,
                    Role.User => RoleDto.User,
                    Role.Assistant => RoleDto.Assistant,
                    _ => throw new InvalidOperationException($"Unsupported message role '{m.Role}'.")
                };

                return new MessageDto(CleanContent(m.Content, conversationVariables), role);
            })
            .ToList();

        return new GenerateCompletionRequest([
            new MessageDto(cleanedSystemPrompt, RoleDto.System),
            new MessageDto(cleanedContext, RoleDto.System),
            .. cleanedMessages
        ]);
    }

    private static Dictionary<string, string> GetConversationVariables(ConversationInput conversation)
    {
        return new Dictionary<string, string>
        {
            { "{{user}}", conversation.User.Name },
            { "{{assistant}}", conversation.Assistant.Name },
            { "{{user_age}}", conversation.User.Age.ToString() },
            { "{{assistant_age}}", conversation.Assistant.Age.ToString() },
            { "{{user_details}}", GetCharacterDescription(conversation.User) },
            { "{{assistant_details}}", GetCharacterDescription(conversation.Assistant) }
        };
    }

    private static string GetCharacterDescription(CharacterInput character)
    {
        List<string> details =
        [
            $"{character.Name} is {character.Age} years old."
        ];
        details.AddRange(character.Details.Select(d => $"{character.Name}'s {d.Key} is {d.Value}"));

        return string.Join(". ", details);
    }

    private static string CleanContent(string content, Dictionary<string, string> variables)
    {
        var result = content;
        foreach (var (variableKey, variableValue) in variables)
        {
            result = result.Replace(variableKey, variableValue);
        }

        return result;
    }
}