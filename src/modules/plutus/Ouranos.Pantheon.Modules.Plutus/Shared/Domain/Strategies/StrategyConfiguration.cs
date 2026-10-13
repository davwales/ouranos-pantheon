using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

/// <summary>
///     Everything a strategy engine needs to trade. Each <see cref="StrategyMode" /> has its own
///     configuration type; the <c>mode</c> discriminator selects it over the API and in storage.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "mode")]
[JsonDerivedType(typeof(SwingConfiguration), nameof(StrategyMode.Swing))]
[JsonDerivedType(typeof(FlipConfiguration), nameof(StrategyMode.Flip))]
public abstract record StrategyConfiguration
{
    [JsonIgnore]
    public abstract StrategyMode Mode { get; }

    /// <summary>Parameter keys the optimizer must keep at their configured values.</summary>
    public IReadOnlyList<string> LockedParameters
    {
        get;
        init => field = value ?? [];
    } = [];

    public abstract void Validate();

    /// <summary>Compares locks by content, since deserialized lists never share a reference.</summary>
    public virtual bool Equals(StrategyConfiguration? other)
    {
        return other is not null
            && EqualityContract == other.EqualityContract
            && LockedParameters.SequenceEqual(other.LockedParameters);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EqualityContract, LockedParameters.Count);
    }
}
