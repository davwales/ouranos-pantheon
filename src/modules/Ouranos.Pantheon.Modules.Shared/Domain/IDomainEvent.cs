namespace Ouranos.Pantheon.Modules.Shared.Domain;

/// <summary>
/// Marker interface for domain events emitted by event-sourced aggregates.
/// Enables type-safe event collections (e.g. <see cref="OperationResult{TAggregate}.Events"/>).
/// </summary>
public interface IDomainEvent { }
