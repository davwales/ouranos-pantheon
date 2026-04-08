namespace Ouranos.Pantheon.Modules.Hermes.Shared;

public sealed record HermesOptions(
    string ConversationNameSystemPrompt,
    string ConversationNameModel
)
{
    public const string SectionName = "Ouranos:Hermes";

    public HermesOptions() : this(
        ConversationNameSystemPrompt: """
                                      Generate a short, descriptive title (maximum 5 words) for the following message.
                                      Respond with only the title - no punctuation, no quotes, no explanation.
                                      """,
        ConversationNameModel: string.Empty
    )
    {
    }

    public bool IsConversationNameGenerationEnabled = !string.IsNullOrWhiteSpace(ConversationNameSystemPrompt) &&
                                                      !string.IsNullOrWhiteSpace(ConversationNameModel);
}
