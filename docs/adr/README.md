# Architecture Decision Records

Short-lived context, permanent decisions. Each ADR captures one significant architecture
decision: the context that forced it, the options considered, the decision made, and its
consequences.

## Format

We use [MADR](https://adr.github.io/madr/) (Markdown Architectural Decision Records) in
its short form: title, status, context, decision drivers, considered options, decision,
and consequences. One decision per file, numbered sequentially, never renumbered
or deleted. Superseded records stay in place and point at their replacement.

## Index

| # | Decision | Status |
|---|----------|--------|
| [0000](0000-use-madr-format.md) | Record architecture decisions with MADR | Accepted |
| [0001](0001-modular-monolith-over-microservices.md) | Modular monolith over microservices | Accepted |
| [0002](0002-vertical-slice-architecture-with-wolverine.md) | Vertical Slice Architecture with Wolverine-mediated dispatch | Accepted |
| [0003](0003-schema-per-module-database-isolation.md) | Schema-per-module database isolation on PostgreSQL | Accepted |
| [0004](0004-polyglot-persistence-on-postgresql.md) | Polyglot persistence on one PostgreSQL engine (TimescaleDB + Marten) | Accepted |
| [0005](0005-wolverine-rabbitmq-for-async-pipelines.md) | Wolverine + RabbitMQ for async pipelines with per-message DLQs | Accepted |
| [0006](0006-tickerq-for-scheduling.md) | TickerQ for in-process scheduling | Accepted |
| [0007](0007-self-hosted-llm-inference-via-ouranosml.md) | Self-hosted LLM inference via OuranosMl | Accepted |
| [0008](0008-no-authentication-single-user.md) | No authentication for the single-user, trusted-network deployment | Accepted |
| [0009](0009-opentelemetry-observability-via-otlp.md) | OpenTelemetry traces and metrics exported to Grafana via OTLP | Accepted |
| [0010](0010-server-side-output-caching-for-dashboard-aggregates.md) | Server-side output caching for hot dashboard aggregates | Accepted |
| [0011](0011-container-images-on-ghcr.md) | Container images on GHCR, deployment owned by the infrastructure repo | Accepted |
| [0012](0012-feature-flags-via-flagsmith.md) | Feature flags via Flagsmith; public mode as data-exposure control | Accepted |
| [0013](0013-module-owned-architecture-documentation.md) | Module-owned architecture documentation | Accepted |

## Module Decision Logs

This log holds platform decisions: those that constrain more than one module. A decision
whose consequences stay inside one module goes in that module's log instead, cited as
`<MODULE>-ADR-NNNN`, and is never listed here, so module work does not edit this index
([ADR 0013](0013-module-owned-architecture-documentation.md)).

| Module | Log |
|--------|-----|
| Plutus | [Plutus docs](../../src/modules/plutus/docs/README.md) |
| Hermes | [Hermes docs](../../src/modules/hermes/docs/README.md) |
| Hestia | [Hestia docs](../../src/modules/hestia/docs/README.md) |
| Shared | [Shared docs](../../src/modules/shared/docs/README.md) |

## Process

1. Decide the level: platform here, or the owning module's log (see
   [Maintaining this documentation](../architecture/README.md#maintaining-this-documentation)).
2. Copy the structure of any existing ADR (or MADR's template).
3. Use the next sequential number and a short, decision-shaped title.
4. Link the ADR from the relevant arc42 section. Most decisions belong in
   [Section 4](../architecture/04-solution-strategy.md).
5. If a decision changes, mark the old ADR superseded and write a new one.
