# 2. Architecture Constraints

These constraints are inputs to the decisions recorded in [Section 4](04-solution-strategy.md) and
[Section 9](09-architecture-decisions.md).

## 2.1 Technical Constraints

| Constraint | Detail | Consequence |
|------------|--------|-------------|
| Single backend runtime | .NET 10, ASP.NET Core Minimal APIs | One language/runtime for all server code; no polyglot services |
| Single frontend stack | Next.js 16 / React 19 / TypeScript | One dashboard for all modules |
| Single database engine | PostgreSQL (+ TimescaleDB extension) | All persistence (relational, time-series, and event-sourced via Marten) runs on one database engine |
| Single message broker | RabbitMQ via Wolverine transport | All async processing is AMQP-based; no Kafka/cloud queues |
| Self-hosted AI inference | Locally hosted LLM exposed through OuranosMl (OpenAI-compatible) | AI features must degrade gracefully when the inference host is offline; no cloud LLM dependency |
| Homelab hosting | All services run on a private network (`ouranos.local`) | Services cannot assume public-cloud primitives (managed secrets, autoscaling) |

## 2.2 Organizational Constraints

| Constraint | Detail | Consequence |
|------------|--------|-------------|
| Single developer | The project is built and operated by one person | Operational simplicity outranks horizontal scalability; automation (formatting, CI, migrations) substitutes for review capacity |
| No external contributions | No contributions from developers other than the owner are allowed  | No public API stability guarantees or deprecation processes are required |
| Portfolio showcase | The project demonstrates modern .NET architecture patterns | Idiomatic, explainable patterns are preferred over clever one-offs; documentation quality matters |
| Cost-free external data | Market data comes from free/community APIs (Universalis, OSRS Wiki, Alpaca IEX) | Rate limits and data coverage are accepted as-is; no paid data vendors |
| Separate infrastructure repository | Provisioning lives in [`ouranos-infrastructure`](https://github.com/davwales/ouranos-infrastructure) (Docker Compose) | This repository contains no compose/Kubernetes manifests; deployment topology is documented but not versioned here |

## 2.3 Development Process Constraints

Enforced by CI and repository hooks (see `.github/workflows/ci.yml`, `automation/`):

- Central package management (`Directory.Packages.props`)
- `.editorconfig` style rules (file-scoped namespaces, braces, sealed types; CI fails on warnings via `dotnet format`)
- CSharpier formatting checked pre-commit; `dotnet format style` / `dotnet format analyzers` verified with no changes
- Pre-push `dotnet build`
- Test suite with an 85% line coverage gate
- Images published from `main` only, via GitHub Actions to GHCR using `GITHUB_TOKEN`

## 2.4 Conventions and Tooling

- Vertical Slice layout is a hard convention: every feature is a folder with handler,
  endpoint, and schemas (see [Section 5](05-building-block-view.md))
- Git hooks are installed automatically via MSBuild target in `Directory.Build.props`
- API requests for manual testing are versioned as a Bruno collection in `bruno/`
