using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

public sealed class Conversation : BaseEntity<Id<Conversation>>
{
    private const string DefaultName = "New Conversation";

    private Conversation(Id<Conversation> id)
        : base(id)
    {
        Name = string.Empty;
    }

    public string Name { get; private set; }

    public Id<Persona> PersonaId { get; private set; }

    public Id<ModelConfig> ModelConfigId { get; private set; }

    private Persona? _persona;

    public Persona Persona =>
        _persona ?? throw new NavigationPropertyNotLoadedException<Conversation>();

    private ModelConfig? _modelConfig;

    public ModelConfig ModelConfig =>
        _modelConfig ?? throw new NavigationPropertyNotLoadedException<Conversation>();

    public List<Message> Messages { get; private set; } = [];

    public bool IsPublic { get; private set; }

    public int? InputTokenCount { get; private set; }

    public int? OutputTokenCount { get; private set; }

    public int? TotalTokenCount { get; private set; }

    public List<Trait> Traits { get; private set; } = [];

    public Id<Folder>? FolderId { get; private set; }

    private Folder? _folder;

    public Folder Folder =>
        _folder ?? throw new NavigationPropertyNotLoadedException<Conversation>();

    public static Conversation Create(
        Id<Conversation> id,
        Id<Persona> personaId,
        Id<ModelConfig> modelConfigId,
        IEnumerable<Message> messages,
        IEnumerable<Trait> traits,
        string? name = null,
        bool isPublic = true,
        Persona? persona = null,
        ModelConfig? modelConfig = null,
        Id<Folder>? folderId = null,
        Folder? folder = null
    )
    {
        Guard.Against.Null(personaId);
        Guard.Against.Null(modelConfigId);

        if (persona is not null)
        {
            Guard.Against.InvalidInput(persona, nameof(persona), p => p.Id == personaId);
        }

        if (modelConfig is not null)
        {
            Guard.Against.InvalidInput(
                modelConfig,
                nameof(modelConfig),
                c => c.Id == modelConfigId
            );
        }

        if (folder is not null)
        {
            Guard.Against.InvalidInput(folder, nameof(folder), f => f.Id == folderId);
        }

        var conversation = new Conversation(id)
        {
            Name = string.IsNullOrWhiteSpace(name) ? DefaultName : name.Trim(),
            PersonaId = personaId,
            ModelConfigId = modelConfigId,
            Messages = [.. messages],
            Traits = [.. traits],
            IsPublic = isPublic,
            _persona = persona,
            _modelConfig = modelConfig,
            FolderId = folderId,
            _folder = folder,
        };

        return conversation;
    }

    public void Update(
        string name,
        Id<Persona> personaId,
        Id<ModelConfig> modelConfigId,
        IEnumerable<Message> messages,
        IEnumerable<Trait> traits,
        bool isPublic,
        Id<Folder>? folderId
    )
    {
        Guard.Against.Null(personaId);
        Guard.Against.Null(modelConfigId);

        Name = string.IsNullOrWhiteSpace(name) ? DefaultName : name.Trim();
        PersonaId = personaId;
        ModelConfigId = modelConfigId;
        Messages = [.. messages];
        Traits = [.. traits];
        IsPublic = isPublic;
        FolderId = folderId;
        Update();
    }

    public void RecordTokenUsage(int inputTokens, int outputTokens, int totalTokens)
    {
        InputTokenCount = inputTokens;
        OutputTokenCount = outputTokens;
        TotalTokenCount = totalTokens;
        Update();
    }
}
