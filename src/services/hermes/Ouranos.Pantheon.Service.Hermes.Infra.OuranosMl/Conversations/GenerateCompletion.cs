using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;
using Ouranos.Pantheon.Service.Hermes.Domain.Conversations;
using Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl.Requests;

namespace Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl.Conversations;

public sealed class GenerateCompletion : IGenerateCompletion
{
    private readonly ILogger<GenerateCompletion> _logger;
    private readonly IOuranosMachineLearningClient _ouranosClient;

    public GenerateCompletion(
        ILogger<GenerateCompletion> logger,
        IOuranosMachineLearningClient ouranosClient
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(ouranosClient);

        _logger = logger;
        _ouranosClient = ouranosClient;
    }

    public async IAsyncEnumerable<string> GenerateCompletionStream(
        Conversation conversation,
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

    private static GenerateCompletionRequest GetRequest(Conversation conversation)
    {
        var conversationVariables = GetConversationVariables(conversation);

        var systemPrompt = "{{user_details}}\n{{assistant_details}}"; // TODO - Retrieve system prompt from somewhere.

        var cleanedSystemPrompt = CleanContent(systemPrompt, conversationVariables);
        var cleanedContext = CleanContent(conversation.Context, conversationVariables);

        var cleanedMessages = conversation.Messages
            .Select(m => new Message(CleanContent(m.Content, conversationVariables), m.Role))
            .ToList();

        return new GenerateCompletionRequest([
            new Message(cleanedSystemPrompt, Role.System),
            new Message(cleanedContext, Role.System),
            .. cleanedMessages
        ]);
        ;
    }

    private static Dictionary<string, string> GetConversationVariables(Conversation conversation)
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

    private static string GetCharacterDescription(Character character)
    {
        List<string> details = [];

        details.Add($"{character.Name} is {character.Age} years old.");
        foreach (var d in character.Details) details.Add($"{character.Name}'s {d.Key} is {d.Value}");

        return string.Join(". ", details);
    }

    private static string CleanContent(string content, Dictionary<string, string> variables)
    {
        var result = content;
        foreach (var (variableKey, variableValue) in variables) result = result.Replace(variableKey, variableValue);
        return result;
    }
}