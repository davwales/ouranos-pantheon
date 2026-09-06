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

## Process

1. Copy the structure of any existing ADR (or MADR's template).
2. Use the next sequential number and a short, decision-shaped title.
3. Link the ADR from the relevant arc42 section. Most decisions belong in
   [Section 4](../architecture/04-solution-strategy.md).
4. If a decision changes, mark the old ADR superseded and write a new one.