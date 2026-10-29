# 12. Glossary

## Platform

| Term | Definition |
|------|------------|
| **Pantheon** | The platform as a whole: gateway + interface + modules |
| **Module** | A self-contained domain unit implementing `IPantheonModule`; references only the shared kernel |
| **Gateway** | The .NET host that composes all modules into one deployable REST API |
| **Interface** | The Next.js dashboard application |
| **Shared kernel** | Abstractions and generic infrastructure referenced by every module (`Ouranos.Pantheon.Modules.Shared.Contract`) |
| **Shared module** | `Ouranos.Pantheon.Modules.Shared`, an `IPantheonModule` referenced only by the gateway that owns platform features (core composition, health, messaging and flag wiring, observability, TickerQ store). Distinct from the shared kernel |
| **Vertical slice** | A feature folder containing handler, endpoint, and schemas, organized end-to-end rather than by technical layer |

## Module Vocabularies

Each module is a bounded context with its own ubiquitous language, kept in its module
glossary. The same word may mean different things in different modules: *Recipe* is a
crafting recipe for cost analysis in Plutus and a cooking recipe in Hestia. Context
determines which is meant.

| Module | Glossary |
|--------|----------|
| Plutus | [Plutus docs](../../src/modules/plutus/docs/README.md) |
| Hermes | [Hermes docs](../../src/modules/hermes/docs/README.md) |
| Hestia | [Hestia docs](../../src/modules/hestia/docs/README.md) |
| Shared | [Shared docs](../../src/modules/shared/docs/README.md) |

## Technical

| Term | Definition |
|------|------------|
| **Wolverine** | .NET message-handling framework: in-process handler dispatch + RabbitMQ transport |
| **Marten** | Event-sourcing + document database library running on PostgreSQL (used by Hestia) |
| **TickerQ** | In-process scheduler with EF Core store and dashboard (all recurring jobs) |
| **Hypertable** | TimescaleDB table partitioned by time, used for trades, signals, and forecasts |
| **Continuous aggregate (cagg)** | TimescaleDB materialized view maintained incrementally over a hypertable |
| **Output cache** | ASP.NET Core server-side HTTP response cache, enabled per endpoint on hot reads |
| **Genetic algorithm** | Generic evolutionary search in the kernel (`Algorithms/Genetic`) |
| **OuranosMl** | Self-hosted ML inference service exposing an OpenAI-compatible API and a custom forecasting endpoint |
| **Flagsmith** | Feature-flag service |
| **ADR** | Architecture Decision Record |
| **MADR** | Markdown Architecture Decision Record, the lightweight ADR template used in [`docs/adr/`](../adr/README.md) |
| **arc42** | The architecture documentation template this documentation follows, at platform and module level |
| **Module docs** | A module's own arc42 subset and ADR log in `src/modules/<module>/docs/`, entered through its `README.md` |

## Abbreviations

| Abbreviation | Meaning |
|--------------|---------|
| CORS | Cross-Origin Resource Sharing |
| DDD | Domain-Driven Design |
| DLQ | Dead-letter queue: the `.dlq`-suffixed queue receiving messages after retries are exhausted |
| GA | Genetic algorithm |
| GHCR | GitHub Container Registry, where CI publishes the gateway and interface images |
| LLM | Large language model |
| ML | Machine learning |
| OTLP | OpenTelemetry Protocol, used to export traces and metrics |
| SSE | Server-Sent Events, used for streaming responses |
| SSRF | Server-side request forgery; the recipe scraper uses an anti-SSRF HTTP handler |
| VSA | Vertical Slice Architecture |
| WSS | WebSocket Secure |
