using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

public sealed class Assistant : BaseEntity<Id<Assistant>>
{
    public Assistant(
        Id<Assistant> id,
        string model,
        string systemPrompt,
        string? assistantName = null,
        string? userName = null,
        float? temperature = null,
        int? maxTokens = null,
        float? repeatPenalty = null
    ) : base(id)
    {
        Guard.Against.NullOrWhiteSpace(model);
        Guard.Against.NullOrWhiteSpace(systemPrompt);

        Model = model;
        SystemPrompt = systemPrompt;
        Temperature = temperature;
        MaxTokens = maxTokens;
        RepeatPenalty = repeatPenalty;

        if (!string.IsNullOrWhiteSpace(assistantName))
        {
            AssistantName = assistantName;
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            UserName = userName;
        }
    }

    public string Model { get; private set; }

    public string SystemPrompt { get; private set; }

    public string AssistantName { get; private set; } = "Assistant";

    public string UserName { get; private set; } = "User";

    public float? Temperature { get; private set; }

    public int? MaxTokens { get; private set; }

    public float? RepeatPenalty { get; private set; }

    public void Update(
        string model,
        string systemPrompt,
        string? assistantName = null,
        string? userName = null,
        float? temperature = null,
        int? maxTokens = null,
        float? repeatPenalty = null
    )
    {
        Guard.Against.NullOrWhiteSpace(model);
        Guard.Against.NullOrWhiteSpace(systemPrompt);

        Model = model;
        SystemPrompt = systemPrompt;
        Temperature = temperature;
        MaxTokens = maxTokens;
        RepeatPenalty = repeatPenalty;

        if (!string.IsNullOrWhiteSpace(assistantName))
        {
            AssistantName = assistantName;
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            UserName = userName;
        }
    }
}