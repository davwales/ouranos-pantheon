# ADR 0005: Wolverine + RabbitMQ for async pipelines with per-message DLQs

**Status:** accepted

## Context

Plutus ingests continuous trade streams from three providers; backtests and recipe
imports are long-running jobs that must not run inside an HTTP request. The system needs
durable, retryable background work with observable failure paths, on infrastructure the
homelab already runs.

## Decision Drivers

- Durable delivery between ingestion and persistence
- Per-message-type queues so one slow/failing consumer cannot starve others
- Explicit, visible failure handling (no silent drops)
- Same framework as in-process handler dispatch (one mental model)

## Considered Options

- **Wolverine + RabbitMQ**: message bus and transport in one framework, handler
  discovery shared with the dispatch pattern of [0002](0002-vertical-slice-architecture-with-wolverine.md)
- MassTransit + RabbitMQ: comparable transport story, separate abstractions from the
  in-process dispatch
- Raw RabbitMQ client: full control, much more plumbing
- Hangfire/queue-in-database: durable jobs, but a second scheduling paradigm

## Decision

Use **Wolverine over RabbitMQ** for all async flows. Conventional local routing is
disabled; each message type declares its topology as constants (exchange, queue, DLQ) and
each message gets its own exchange→queue binding with a `.dlq` dead-letter queue.
Topology is auto-provisioned at startup. Error policy (centralized): expected
`ArgumentException`/`InvalidOperationException` go straight to the error queue; anything
else retries on a fixed cooldown before dead-lettering.

## Consequences

- Topology is code-reviewable at the message type
- Failures are classed explicitly: bug vs. transient
- Backtest/import flows use HTTP 202 + polling of persisted progress rather than
  callback messaging, which is simpler and works for the UI
- In-process dispatch never touches the transport: `IMessageBus.InvokeAsync` commands and
  queries run inline against locally discovered handlers; only `PublishAsync` flows (trade
  messages, backtests, recipe imports) go through RabbitMQ
- The broker is still required at gateway startup: whenever `Ouranos:RabbitMq:Host` is set,
  the RabbitMQ transport is configured with `AutoProvision()` and queue listeners, so
  Wolverine connects and declares topology during bootstrapping; development uses the local
  compose stack
