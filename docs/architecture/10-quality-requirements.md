# 10. Quality Requirements

## 10.1 Quality Tree

Quality goals from [Section 1.2](01-introduction-and-goals.md#12-quality-goals),
decomposed:

```mermaid
graph TB
    root["Platform quality"]

    ext["Extensibility (Q1)"]
    clar["Design clarity (Q2)"]
    ops["Operational simplicity (Q3)"]
    resp["Responsiveness (Q4)"]

    root --> ext
    root --> clar
    root --> ops
    root --> resp

    ext1["New domain =<br/>new module only"] --> ext
    ext2["No cross-module<br/>assembly references"] --> ext
    ext3["Shared kernel stays<br/>domain-free"] --> ext

    clar1["Every decision has<br/>a recorded rationale"] --> clar
    clar2["Feature = one readable<br/>vertical slice"] --> clar
    clar3["Docs updated with<br/>architectural change"] --> clar

    ops1["One deployable; stack<br/>in ouranos-infrastructure"] --> ops
    ops2["85% .NET coverage<br/>gate in CI"] --> ops
    ops3["Health checks for all<br/>critical dependencies"] --> ops
    ops4["Auto-provisioned<br/>messaging topology"] --> ops

    resp1["TimescaleDB aggregates<br/>for market views"] --> resp
    resp2["Output cache on hot<br/>Plutus reads"] --> resp
    resp3["Long work async<br/>(202 + poll)"] --> resp
```

## 10.2 Quality Scenarios

Draft scenarios, each concrete and verifiable against the running system:

| # | Goal | Scenario | Metric / target |
|---|------|----------|-----------------|
| S1 | Q1 | **Add a new domain module** | Time from empty project to first working endpoint without touching existing modules; validated by the module contract (kernel reference only) |
| S2 | Q3 | **Trade ingestion resilience** | When an external WebSocket drops, the `WebSocketWorker` reconnects and health shows the outage; WebSocket trades during the outage are not replayed; failing messages land in `.dlq` rather than disappearing |
| S3 | Q4 | **Backtest responsiveness** | An HTTP backtest or optimization submission returns 202 in < 1 s; the computation proceeds asynchronously with persisted progress visible in `GET /backtests/{id}` |
| S4 | Q2 | **Query consistency** | List endpoints share the same paging/sorting/filter contract (exceptions in [8.2](08-crosscutting-concepts.md#82-application-patterns)) |
| S5 | Q3 | **Regression safety** | CI rejects any change dropping line coverage below 85% |
| S6 | Q3 | **Dependency health visibility** | All critical dependencies (Postgres, RabbitMQ, OuranosMl, WebSocket connectivity, TickerQ) are observable via health checks |
| S7 | Q4 | **Dashboard aggregates at volume** | Market overview and volume heatmap stay interactive at production trade volumes while a page polls them: repeat requests within the cache TTL are served without touching Postgres ([ADR 0010](../adr/0010-server-side-output-caching-for-dashboard-aggregates.md)) |
