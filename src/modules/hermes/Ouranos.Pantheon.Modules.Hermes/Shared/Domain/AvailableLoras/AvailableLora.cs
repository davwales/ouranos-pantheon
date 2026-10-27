using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;

/// <summary>
/// A LoRA the image server offers, synced from OuranosMl. LoRA configs pick from these.
/// </summary>
public class AvailableLora : BaseEntity<Id<AvailableLora>>
{
    protected AvailableLora(Id<AvailableLora> id)
        : base(id)
    {
        Identifier = string.Empty;
    }

    public string Identifier { get; private set; }

    public string? Title { get; private set; }

    public string? CompatClass { get; private set; }

    public float? DefaultWeight { get; private set; }

    public string? TriggerPhrase { get; private set; }

    public string? UsageHint { get; private set; }

    /// <summary>
    /// Whether the LoRA suits the configured image model. True when either side's compat class is
    /// unknown, since only a known mismatch is worth warning about.
    /// </summary>
    public bool IsCompatible { get; private set; }

    public static AvailableLora Create(
        Id<AvailableLora> id,
        string identifier,
        string? title,
        string? compatClass,
        float? defaultWeight,
        string? triggerPhrase,
        string? usageHint,
        bool isCompatible
    )
    {
        Guard.Against.NullOrWhiteSpace(identifier);

        return new AvailableLora(id)
        {
            Identifier = identifier,
            Title = title,
            CompatClass = compatClass,
            DefaultWeight = defaultWeight,
            TriggerPhrase = triggerPhrase,
            UsageHint = usageHint,
            IsCompatible = isCompatible,
        };
    }

    public void Update(
        string? title,
        string? compatClass,
        float? defaultWeight,
        string? triggerPhrase,
        string? usageHint,
        bool isCompatible
    )
    {
        Title = title;
        CompatClass = compatClass;
        DefaultWeight = defaultWeight;
        TriggerPhrase = triggerPhrase;
        UsageHint = usageHint;
        IsCompatible = isCompatible;
        Update();
    }
}
