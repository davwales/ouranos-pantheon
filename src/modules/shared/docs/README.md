# Shared Module

Shared is two assemblies under `src/modules/shared/`: the shared **kernel**
(`Ouranos.Pantheon.Modules.Shared.Contract`), referenced by every module, and the Shared
**module** (`Ouranos.Pantheon.Modules.Shared`), an `IPantheonModule` referenced only by the
gateway that owns core composition and platform features.

These docs describe how the Shared pieces work inside. The rules every module must follow
(module contract, messaging, persistence and API conventions) are platform concerns and stay
in the [platform documentation](../../../../docs/architecture/README.md), mainly
[Section 8](../../../../docs/architecture/08-crosscutting-concepts.md).

## Contents

| Section | Focus |
|---------|-------|
| [5. Building Block View](05-building-block-view.md) | What each Shared assembly contains |
| [8. Concepts](08-concepts.md) | The AI assistant framework (backend and frontend) |
| [11. Risks and Technical Debt](11-risks-and-technical-debt.md) | Shared-scoped debt (`SHARED-D*`) |
| [12. Glossary](12-glossary.md) | Shared terms |
| [Decisions](adr/README.md) | Module ADR log (`SHARED-ADR-*`) |

## Responsibilities

- Provide the kernel abstractions and generic infrastructure modules build on
- Compose the host: Serilog, REST conventions, Wolverine and RabbitMQ, TickerQ, caching,
  observability, Flagsmith, health checks (`AddOuranosCore` / `UseOuranosCore`)
- Expose `GET /health` and the TickerQ dashboard
- Hold notification entities and their dispatch job

## Context

| Neighbour | Channel | Purpose | Code entry point |
|-----------|---------|---------|------------------|
| OuranosMl | Outbound HTTP (OpenAI-compatible) | Client used by every module | Shared.Contract: `Infra/OuranosMachineLearning/` |
| Flagsmith | Outbound HTTP | Feature-flag client | Shared: `Infra/Flagsmith/` |
| Grafana Alloy | Outbound gRPC (OTLP) | Trace and metric export | Shared: `Infra/Observability/` |
| PostgreSQL | TCP (Npgsql) | Persistence core, TickerQ store, notifications | Shared.Contract: `Infra/Postgres/` |
| RabbitMQ | AMQP | Wolverine transport and error policy | Shared: `API/Extensions/CoreExtensions.cs` |

## Provided Interfaces

- **REST**: `GET /health` (unprefixed), TickerQ dashboard at `/tickerq/dashboard`.
- **Kernel API** for modules: see the [building block view](05-building-block-view.md).
- **Messages**: none.

### Scheduled Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|
| `NotificationSender` | every minute | Dispatch pending notifications |

## Persistence

Schema for `SharedDbContext` (notifications) and the separate `TickerQDbContext` store,
both migrated centrally in `UseOuranosCore`.

## Configuration

Platform sections under `Ouranos:` (`RabbitMq`, `Postgres`, `Query`, `Health`, `Flagsmith`,
`Observability`); listed in
[platform Section 7.3](../../../../docs/architecture/07-deployment-view.md#73-configuration-surface).
Global model defaults for every assistant and LLM feature are code, in `ModelDefaults`
(`Shared.Contract/Infra/OuranosMachineLearning/`).
