using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Hermes.Service.Domain.Assistants;

public class Assistant : BaseEntity<Id<Assistant>>
{
    protected Assistant(Id<Assistant> id) : base(id)
    {
        Model = string.Empty;
        SystemPrompt = string.Empty;
    }

    public string Model { get; private set; }

    public string SystemPrompt { get; private set; }

    public string AssistantName { get; private set; } = "Assistant";

    public string UserName { get; private set; } = "User";

    public float? Temperature { get; private set; }

    public int? MaxTokens { get; private set; }

    public float? RepeatPenalty { get; private set; }

    public static Assistant Create(
        Id<Assistant> id,
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

        var assistant = new Assistant(id)
        {
            Model = model,
            SystemPrompt = systemPrompt,
            Temperature = temperature,
            MaxTokens = maxTokens,
            RepeatPenalty = repeatPenalty
        };

        if (!string.IsNullOrWhiteSpace(assistantName))
        {
            assistant.AssistantName = assistantName;
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            assistant.UserName = userName;
        }

        return assistant;
    }

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