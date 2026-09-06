# 9. Architecture Decisions

Significant architecture decisions are recorded as MADR files in
[`docs/adr/`](../adr/README.md). This section indexes them and highlights the decisions
with the widest reach.

## Index

| # | Decision | Impact |
|---|----------|--------|
| [0000](../adr/0000-use-madr-format.md) | Record architecture decisions with MADR | Process |
| [0001](../adr/0001-modular-monolith-over-microservices.md) | Modular monolith over microservices | Shapes the whole platform |
| [0002](../adr/0002-vertical-slice-architecture-with-wolverine.md) | Vertical Slice Architecture with Wolverine-mediated dispatch | Every feature in every module |
| [0003](../adr/0003-schema-per-module-database-isolation.md) | Schema-per-module database isolation on PostgreSQL | All persistence |
| [0004](../adr/0004-polyglot-persistence-on-postgresql.md) | Polyglot persistence on one PostgreSQL engine (TimescaleDB + Marten) | Plutus (TimescaleDB) and Hestia (Marten) |
| [0005](../adr/0005-wolverine-rabbitmq-for-async-pipelines.md) | Wolverine + RabbitMQ for async pipelines with per-message DLQs | Ingestion, backtests, recipe import |
| [0006](../adr/0006-tickerq-for-scheduling.md) | TickerQ for in-process scheduling | All recurring jobs |
| [0007](../adr/0007-self-hosted-llm-inference-via-ouranosml.md) | Self-hosted LLM inference via OuranosMl | All domains' AI capability |
| [0008](../adr/0008-no-authentication-single-user.md) | No authentication for the single-user, trusted-network deployment | Gateway surface + frontend |

## The Decisions That Shape Everything

Three decisions explain most of the architecture's texture:

1. **[ADR 0001](../adr/0001-modular-monolith-over-microservices.md), modular monolith**:
   the module is the unit of extension; the gateway composes; nothing else is shared.
2. **[ADR 0002](../adr/0002-vertical-slice-architecture-with-wolverine.md), vertical slices + Wolverine**:
   the unit of change is the feature folder; the message bus is the only slice-to-slice
   path.
3. **[ADR 0004](../adr/0004-polyglot-persistence-on-postgresql.md), polyglot persistence on one engine**:
   each domain gets the persistence style it needs (hypertables, event streams, plain
   relational) without multiplying infrastructure.

New decisions follow the process in [`docs/adr/README.md`](../adr/README.md).
