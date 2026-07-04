namespace Ouranos.Pantheon.Modules.Shared.Domain;

/// <summary>
/// Base type for event-sourced aggregates persisted as Marten inline snapshots.
/// Aggregates are immutable records; state evolves via static
/// <c>Create(TEvent)</c> / <c>Apply(TEvent, TAggregate)</c> methods (Marten convention).
/// <see cref="Id"/> is the Marten event stream key.
/// </summary>
public abstract record BaseEventSourcedEntity
{
    public Guid Id { get; init; }
}
