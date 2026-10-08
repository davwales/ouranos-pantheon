# 7. Deployment View

## 7.1 Deployment Artifacts

Exactly two container images are built and published (`.github/workflows/ci.yml`,
`publish` job, `main` branch only; [ADR 0011](../adr/0011-container-images-on-ghcr.md)):

| Image | Base | Port | Source |
|-------|------|------|--------|
| `ghcr.io/davwales/ouranos-pantheon/gateway` | `aspnet:10.0` | 8300 | `src/apps/gateway/Dockerfile` |
| `ghcr.io/davwales/ouranos-pantheon/interface` | `node:22-alpine` | 3000 | `src/apps/interface/Dockerfile` |

Pushes use buildx with a registry cache (`:cache` tag) and `GITHUB_TOKEN`, so there are no registry
secrets.

## 7.2 Environment Topology

Infrastructure (PostgreSQL/TimescaleDB, RabbitMQ, Flagsmith, Loki, Grafana Alloy) is
provisioned by the separate [`ouranos-infrastructure`](https://github.com/davwales/ouranos-infrastructure)
repository via Docker Compose. This repository assumes those services exist and configures
connections through `appsettings.*.json`; service hostnames (e.g. the Loki sink target
`loki-gateway:3100`) come from that compose project's DNS names. OuranosMl is not part of
that stack: it is a separate system on its own host.

```mermaid
graph TB
    user(["Owner (browser)"])

    subgraph lan["Homelab LAN"]
        subgraph host["Homelab host: ouranos.local (ouranos-infrastructure compose)"]
            subgraph apps["Application containers"]
                ifc["Interface container<br/>:3000"]
                gw["Gateway container<br/>:8300"]
            end

            subgraph infra["Backing services"]
                pg[("PostgreSQL +<br/>TimescaleDB")]
                mq[["RabbitMQ"]]
                fs["Flagsmith"]
                loki["Grafana Loki"]
                alloy["Grafana Alloy<br/>(OTLP receiver)"]
            end
        end

        ml["OuranosMl<br/>(separate inference host)"]
    end

    subgraph external["External APIs (internet)"]
        uni["Universalis (WSS)"]
        osrs["OSRS Wiki API"]
        alp["Alpaca IEX (WSS)"]
        xiv["XIVAPI item data<br/>(static GitHub dump)"]
        web["Recipe websites"]
    end

    user -- "loads UI" --> ifc
    user -- "REST / SSE (CORS)" --> gw

    gw --> pg
    gw --> mq
    gw --> fs
    gw -- "Serilog sink" --> loki
    gw -- "OTLP gRPC" --> alloy
    gw -- "HTTP" --> ml
    gw --> external
```

## 7.3 Configuration Surface

| Concern | Mechanism |
|---------|-----------|
| Connections (Postgres, RabbitMQ, OuranosMl, Flagsmith, Loki) | `appsettings.*.json` per environment, overridable by environment variables |
| CORS allow-list | `CorsAllowedHosts` array (policy `AllowLocalAndServer`) |
| Wolverine retry policy | `Ouranos:RabbitMq:RetryCount` (the policy is registered only when it is set): cooldown retries for unexpected exceptions, immediate dead-letter for `ArgumentException`/`InvalidOperationException` |
| Query defaults | `Ouranos:Query` section (`QueryOptions`: paging/sorting limits behind the common query contract) |
| Data loaders | `Ouranos:Plutus:DataLoaders` enable flags (all default to enabled) |
| Forecasting | `Ouranos:Plutus:Forecasting` enable flag and model settings |
| Feature flags | Flagsmith environments (dev at `ouranos.local:8001`), see [ADR 0012](../adr/0012-feature-flags-via-flagsmith.md) |
| Trace and metric export | `Ouranos:Observability` section (`ObservabilityOptions`): endpoint/protocol/sampling, see [ADR 0009](../adr/0009-opentelemetry-observability-via-otlp.md) |
| Frontend API base | `NEXT_PUBLIC_API_BASE` / `NEXT_PUBLIC_API_HOST` (default `http://localhost:8300`), inlined at build time |

## 7.4 CI/CD

```mermaid
graph LR
    lint["lint: csharpier check +<br/>dotnet format (verify-no-changes)"] --> build["build:<br/>dotnet build"]
    build --> test["test: 85% coverage<br/>+ reportgenerator"]
    test --> publish["publish (main only):<br/>GHCR images"]
```

The lint and build jobs also run the frontend steps (`npm run lint`, `npm run build`).
There is no automated deployment step: promoting a release (pulling new images on the
homelab host) is a manual operation, consistent with the single-operator constraint of
[Section 2](02-architecture-constraints.md).
