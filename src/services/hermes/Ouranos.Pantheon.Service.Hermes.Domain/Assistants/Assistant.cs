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
        string? userName = null
    ) : base(id)
    {
        Guard.Against.NullOrWhiteSpace(model);
        Guard.Against.NullOrWhiteSpace(systemPrompt);

        Model = model;
        SystemPrompt = systemPrompt;

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

    public void Update(
        string model,
        string systemPrompt,
        string? assistantName = null,
        string? userName = null
    )
    {
        Guard.Against.NullOrWhiteSpace(model);
        Guard.Against.NullOrWhiteSpace(systemPrompt);

        Model = model;
        SystemPrompt = systemPrompt;

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