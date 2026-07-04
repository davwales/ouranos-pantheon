namespace Ouranos.Pantheon.Modules.Shared.Domain;

/// <summary>
/// Immutable result of a domain operation on an event-sourced aggregate: the new
/// aggregate state plus the domain events the operation requires to be appended.
/// Reusable across all event-sourced aggregates and operations.
/// </summary>
public sealed record OperationResult<TAggregate>(
    TAggregate State,
    IReadOnlyList<IDomainEvent> Events
)
    where TAggregate : BaseEventSourcedEntity
{
    /// <summary>
    /// Convenience factory for the common case of one or more events emitted by an operation.
    /// </summary>
    public static OperationResult<TAggregate> Of(TAggregate state, params IDomainEvent[] events)
    {
        return new(state, events);
    }
}
