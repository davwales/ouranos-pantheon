using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Talos.Olympus.Service.Aphrodite.Application.Interfaces.Conversations;
using Talos.Olympus.Service.Aphrodite.Domain.Characters;
using Talos.Olympus.Service.Aphrodite.Domain.Conversations;
using Talos.Olympus.Service.Aphrodite.Infra.TalosMl.Requests;

namespace Talos.Olympus.Service.Aphrodite.Infra.TalosMl.Conversations;

public sealed class GenerateCompletion : IGenerateCompletion
{
    private readonly ILogger<GenerateCompletion> _logger;
    private readonly ITalosMachineLearningClient _talosClient;

    public GenerateCompletion(
        ILogger<GenerateCompletion> logger,
        ITalosMachineLearningClient talosClient
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(talosClient);

        _logger = logger;
        _talosClient = talosClient;
    }

    public async IAsyncEnumerable<string> GenerateCompletionStream(
        Conversation conversation,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to generate a completion for conversation '{@conversation}'.", conversation);
        cancellationToken.ThrowIfCancellationRequested();

        var request = GetRequest(conversation);
        await foreach (var line in _talosClient.GenerateCompletion(request, cancellationToken))
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