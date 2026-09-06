# ADR 0002: Vertical Slice Architecture with Wolverine-mediated dispatch

**Status:** accepted

## Context

Feature logic in classic layered architectures (controller → service → repository) spreads
one behavior across three files in three folders. With many features across four modules,
this makes both reading and changing a feature expensive. Handlers also need to be
discoverable without a hand-maintained mediator registry.

## Decision Drivers

- A feature should be readable end-to-end in one place
- Adding a feature must not require modifying shared dispatch code
- No cross-slice direct references, even inside a module
- Convention-based discovery rather than registration lists

## Considered Options

- **Vertical slices + Wolverine `IPantheonHandler` markers**: feature folders; Wolverine
  discovers handlers by marker interface across module assemblies
- Layered architecture per module: controllers/services/repositories
- MediatR with manual assembly scanning: similar discovery, different ecosystem

## Decision

Every feature is a **vertical slice**: `<Name>Handler` (`IPantheonHandler<TInput,
TOutput>` or the fire-and-forget/streaming variants), `<Name>Endpoint` (static
`Map(WebApplication)`), and sealed record schemas under `Schemas/`. Endpoints typically dispatch
through Wolverine's `IMessageBus.InvokeAsync<T>`. Handler discovery scans all
module assemblies for the marker interfaces; there is no per-feature registration.

## Consequences

- New feature = new folder; zero changes to existing dispatch code
- Slices share no service abstractions, so they stay decoupled by construction
- Domain invariants live with aggregates (DDD), not in "service" classes
- Wolverine conventions (naming, method signatures) become load-bearing and are
  documented in arc42 [Section 8.2](../architecture/08-crosscutting-concepts.md#82-application-patterns)
