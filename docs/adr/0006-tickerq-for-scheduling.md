# ADR 0006: TickerQ for in-process scheduling

**Status:** accepted

## Context

Recurring work exists across modules: market polling (OSRS every 5 minutes), signal
recalculation (every 5 minutes), forecast generation (daily), model sync (hourly),
notification dispatch (per second). The scheduler must persist schedules across restarts
and be observable, without adding a second distributed component.

## Decision Drivers

- Persisted, restart-safe schedules
- Dashboard/observability for cron jobs
- Same database engine as everything else
- Job code lives with the module that owns it

## Considered Options

- **TickerQ**: in-process scheduler, EF Core operational store, built-in dashboard
- Quartz.NET: more features, heavier, its own store conventions
- Hangfire: job-queue oriented with its own dashboard; overlaps the Wolverine/RabbitMQ
  async role of [0005](0005-wolverine-rabbitmq-for-async-pipelines.md)
- Cron + shell: external to the app, no typed job code

## Decision

Use **TickerQ** for all recurring work. The operational store is EF Core on the shared
Postgres instance (migrated centrally in `UseOuranosCore`); the dashboard is exposed at
`/tickerq/dashboard`. Jobs are typed classes in the owning module; long-running jobs
guard against overlapping ticks. It aligns well with existing infrastructure and offers a modern interface when compared to Hangfire.

## Consequences

- One scheduler, one store, zero extra infrastructure
- Jobs are in-process: they scale with the gateway and fail with it (acceptable for a
  monolith per [0001](0001-modular-monolith-over-microservices.md))
- Multi-instance deployments would need job-level coordination (not currently needed)
