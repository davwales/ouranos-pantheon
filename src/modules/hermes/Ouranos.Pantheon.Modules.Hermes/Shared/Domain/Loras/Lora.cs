using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;

public class Lora : BaseEntity<Id<Lora>>
{
    public const float DefaultWeight = 1f;

    public const float MinWeight = -2f;

    public const float MaxWeight = 2f;

    protected Lora(Id<Lora> id)
        : base(id)
    {
        Name = string.Empty;
        Identifier = string.Empty;
    }

    public string Name { get; private set; }

    public string Identifier { get; private set; }

    public float Weight { get; private set; }

    /// <summary>
    /// Added to the image prompt while the LoRA is active; prefilled from the synced LoRA.
    /// </summary>
    public string? TriggerPhrase { get; private set; }

    public bool IsPublic { get; private set; }

    public static Lora Create(
        Id<Lora> id,
        string name,
        string identifier,
        float weight = DefaultWeight,
        bool isPublic = true,
        string? triggerPhrase = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(identifier);
        Guard.Against.OutOfRange(weight, nameof(weight), MinWeight, MaxWeight);

        return new Lora(id)
        {
            Name = name.Trim(),
            Identifier = identifier.Trim(),
            Weight = weight,
            IsPublic = isPublic,
            TriggerPhrase = NormalizeTriggerPhrase(triggerPhrase),
        };
    }

    public void Update(
        string name,
        string identifier,
        float weight,
        bool isPublic = true,
        string? triggerPhrase = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(identifier);
        Guard.Against.OutOfRange(weight, nameof(weight), MinWeight, MaxWeight);

        Name = name.Trim();
        Identifier = identifier.Trim();
        Weight = weight;
        IsPublic = isPublic;
        TriggerPhrase = NormalizeTriggerPhrase(triggerPhrase);
        Update();
    }

    private static string? NormalizeTriggerPhrase(string? triggerPhrase)
    {
        return string.IsNullOrWhiteSpace(triggerPhrase) ? null : triggerPhrase.Trim();
    }
}
