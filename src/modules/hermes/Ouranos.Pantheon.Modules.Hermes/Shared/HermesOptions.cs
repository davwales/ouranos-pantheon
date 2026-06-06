namespace Ouranos.Pantheon.Modules.Hermes.Shared;

public sealed record HermesOptions(
    string ConversationNameSystemPrompt,
    string ConversationNameModel,
    string CompactionSummaryPrompt,
    float CompactionTemperature,
    int CompactionMaxTokens
)
{
    public const string SectionName = "Ouranos:Hermes";

    public const string DefaultCompactionSummaryPrompt = """
        You are summarizing a conversation for context compaction.
        The conversation is with {PersonaName}: {PersonaDescription}

        Produce a structured summary with these sections:

        ### Intent
        What the user was trying to accomplish.

        ### Key Concepts
        Important facts, preferences, constraints, or decisions established.

        ### Errors & Fixes
        Any errors encountered and how they were (or weren't) resolved.

        ### User Messages
        Brief summary of each key user communication and request.

        ### Pending Tasks
        Unresolved questions, open items, or tasks not yet completed.

        ### Current Work
        What was happening at the end of the conversation.

        Be concise but thorough. The summary will replace the full history for future context.
        """;

    public const float DefaultCompactionTemperature = 0.3f;

    public const int DefaultCompactionMaxTokens = 1024;

    public HermesOptions()
        : this(
            ConversationNameSystemPrompt: """
            Generate a short, descriptive title (maximum 5 words) for the following message.
            Respond with only the title - no punctuation, no quotes, no explanation.
            """,
            ConversationNameModel: string.Empty,
            CompactionSummaryPrompt: DefaultCompactionSummaryPrompt,
            CompactionTemperature: DefaultCompactionTemperature,
            CompactionMaxTokens: DefaultCompactionMaxTokens
        ) { }

    public bool IsConversationNameGenerationEnabled =
        !string.IsNullOrWhiteSpace(ConversationNameSystemPrompt)
        && !string.IsNullOrWhiteSpace(ConversationNameModel);

    public string EffectiveCompactionSummaryPrompt =>
        string.IsNullOrWhiteSpace(CompactionSummaryPrompt)
            ? DefaultCompactionSummaryPrompt
            : CompactionSummaryPrompt;

    public float EffectiveCompactionTemperature =>
        CompactionTemperature is < 0f or > 2f
            ? DefaultCompactionTemperature
            : CompactionTemperature;

    public int EffectiveCompactionMaxTokens =>
        CompactionMaxTokens <= 0 ? DefaultCompactionMaxTokens : CompactionMaxTokens;
}
