# 6. Runtime View

This section traces the system's key scenarios. All of them are driven by the building
blocks of [Section 5](05-building-block-view.md) and the conventions of
[Section 8](08-crosscutting-concepts.md).

## 6.1 Scheduled Jobs Overview

Recurring work is coordinated by TickerQ jobs (all in-process, with an EF Core-backed
store, dashboard at `/tickerq/dashboard`):

| Job | Module | Schedule | Purpose |
|-----|--------|----------|---------|
| `OsrsDataLoaderJob` | Plutus | every 5 min | Poll OSRS Wiki prices → publish `TradeMessage`s |
| `SymbolSignalCalculateJob` | Plutus | every 5 min | Recompute signals for all symbols |
| `ForecastGeneratorJob` | Plutus | daily | Generate price forecasts via OuranosMl |
| `SyncModelsJob` | Hermes | hourly | Sync available LLM models from OuranosMl |
| `NotificationSenderJob` | Shared | every second | Dispatch pending notifications |

## 6.2 Scenario: Plain Query Request

*The bread-and-butter flow every list endpoint follows.*

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant EP as REST endpoint
    participant B as Wolverine (IMessageBus)
    participant H as Query handler
    participant DB as Postgres (EF Core)

    UI ->> EP: GET /plutus/markets?filter=...&sort=...&take=...
    EP ->> B: InvokeAsync(input)
    B ->> H: dispatch to registered handler
    H ->> DB: paged / sorted / filtered query
    DB -->> H: rows
    H -->> EP: output record
    EP -->> UI: 200 OK (camelCase JSON)
```

Paging, sorting, and the `field:op:value` filter language follow the common query contract
from [Section 8.2](08-crosscutting-concepts.md#82-application-patterns); failures surface
as RFC 7807 problem details ([Section 8.6](08-crosscutting-concepts.md#86-errors-and-validation)).

## 6.3 Scenario: Real-Time Trade Ingestion

*The continuous pipeline that turns external market events into queryable aggregates.*

```mermaid
sequenceDiagram
    autonumber
    participant U as Universalis / Alpaca (WSS)
    participant L as Listener (Ffxiv / Stocks)
    participant W as WebSocketWorker (hosted service)
    participant X as RabbitMQ exchange plutus.trade
    participant C as TradeConsumer
    participant DB as TimescaleDB

    U ->> L: trade event (BSON / JSON)
    L ->> W: normalized TradeMessage
    W ->> X: publish TradeMessage
    X ->> C: deliver (queue plutus.trade.ingest)
    C ->> DB: upsert symbol, insert trade
    C -->> X: ack (or dead-letter on failure)
```

The OSRS variant replaces steps 1–2 with `OsrsDataLoaderJob` polling on a 5-minute
schedule. Continuous aggregates keep market views query-ready without query-time
aggregation.

## 6.4 Scenario: Signal Calculation and Forecasting

*Every five minutes, computed signals refresh the analytical surface.*

```mermaid
sequenceDiagram
    autonumber
    participant T as TickerQ
    participant J as SymbolSignalCalculateJob
    participant SC as Signal computers
    participant DB as TimescaleDB

    T ->> J: tick (*/5 * * * *)
    J ->> DB: load recent trades / symbols
    J ->> SC: compute (RSI, Bollinger, MAC, Volume Anomaly, ...)
    SC ->> DB: persist signals (hypertable)
```

The daily `ForecastGeneratorJob` follows the same shape but calls OuranosMl
(`POST /plutus/forecast`) and persists `Forecast` records; forecast efficacy is then
evaluated against realized trades.

## 6.5 Scenario: Backtest / Optimization Run

*An HTTP-triggered, long-running computation executed asynchronously.*

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant EP as RunBacktest / Optimize endpoint
    participant X as RabbitMQ exchange plutus.backtest
    participant H as Backtest handler pipeline
    participant GA as Genetic algorithm engine
    participant DB as Postgres

    UI ->> EP: POST strategy backtest / optimize
    EP -->> UI: 202 Accepted { backtestId }
    EP ->> X: RunBacktestMessage / OptimizeStrategyMessage
    X ->> H: deliver (queues .run / .optimize)
    H ->> H: step pipeline (IStep registry)
    opt optimize
        H ->> GA: evolve input weights (Sortino/CAGR/drawdown/turnover objectives)
    end
    H ->> DB: persist progress + results (metrics, optimized weights)
    UI ->> EP: GET backtest status until terminal state
```

The 202-Accepted-then-poll contract is what the Interface relies on. Cancel and restart follow the same message shape over
`plutus.backtest` (the `CancelBacktest`/`RestartBacktest` slices).

## 6.6 Scenario: Recipe Import

*An end-to-end AI-assisted ingestion flow in Hestia.*

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant EP as Import endpoint
    participant X as RabbitMQ exchange hestia.recipe
    participant C as ImportRecipeConsumer
    participant S as RecipeScraper (anti-SSRF)
    participant ML as OuranosMl (structured output)
    participant M as Marten event store

    UI ->> EP: submit recipe URL
    EP -->> UI: 202 Accepted
    EP ->> X: ImportRecipeRequested
    X ->> C: deliver (queue hestia.recipe.import)
    C ->> S: fetch + parse JSON-LD metadata
    C ->> ML: normalize ingredients/steps
    C ->> M: append events to recipe stream
```

## 6.7 Scenario: Streaming Chat

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant EP as Hermes endpoint
    participant H as GenerateCompletionHandler
    participant ML as OuranosMl (OpenAI-compatible)
    participant DB as Postgres (schema hermes)

    UI ->> EP: send message (persona + conversation)
    EP ->> H: GenerateCompletionCommand
    H ->> ML: chat completion stream (persona config)
    ML -->> H: streamed tokens
    H -->> UI: streamed response
    opt conversation provided
        H ->> DB: persist user + assistant messages
    end
```

## 6.8 Observability at Runtime

Health is exposed via module-registered checks (Postgres, RabbitMQ, OuranosMl, WebSocket
connectivity, TickerQ). Structured Serilog logs flow to Loki in production. There is no
OpenTelemetry instrumentation yet. That gap is recorded as debt in
[Section 11](11-risks-and-technical-debt.md).
