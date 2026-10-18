using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;

public class ModelConfig : BaseEntity<Id<ModelConfig>>
{
    protected ModelConfig(Id<ModelConfig> id)
        : base(id)
    {
        Name = string.Empty;
        ModelIdentifier = string.Empty;
        SystemPrompt = string.Empty;
    }

    public string Name { get; private set; }

    public string ModelIdentifier { get; private set; }

    public string SystemPrompt { get; private set; }

    public float? Temperature { get; private set; }

    public int? MaxTokens { get; private set; }

    public Id<ToolSet>? DefaultToolSetId { get; private set; }

    public int? ContextWindow { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsPublic { get; private set; }

    public bool IsUnavailable { get; private set; }

    public static ModelConfig Create(
        Id<ModelConfig> id,
        string name,
        string modelIdentifier,
        string systemPrompt,
        float? temperature = null,
        int? maxTokens = null,
        Id<ToolSet>? defaultToolSetId = null,
        int? contextWindow = null,
        bool isDefault = false,
        bool isPublic = true
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(modelIdentifier);
        Guard.Against.NullOrWhiteSpace(systemPrompt);

        return new ModelConfig(id)
        {
            Name = name,
            ModelIdentifier = modelIdentifier,
            SystemPrompt = systemPrompt,
            Temperature = temperature,
            MaxTokens = maxTokens,
            DefaultToolSetId = defaultToolSetId,
            ContextWindow = contextWindow,
            IsDefault = isDefault,
            IsPublic = isPublic,
            IsUnavailable = false,
        };
    }

    public void Update(
        string name,
        string modelIdentifier,
        string systemPrompt,
        float? temperature = null,
        int? maxTokens = null,
        Id<ToolSet>? defaultToolSetId = null,
        int? contextWindow = null,
        bool isDefault = false,
        bool isPublic = true
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(modelIdentifier);
        Guard.Against.NullOrWhiteSpace(systemPrompt);

        Name = name;
        ModelIdentifier = modelIdentifier;
        SystemPrompt = systemPrompt;
        Temperature = temperature;
        MaxTokens = maxTokens;
        DefaultToolSetId = defaultToolSetId;
        ContextWindow = contextWindow;
        IsDefault = isDefault;
        IsPublic = isPublic;
    }

    public void MarkUnavailable()
    {
        IsUnavailable = true;
        Update();
    }

    public void MarkAvailable()
    {
        IsUnavailable = false;
        Update();
    }
}
