# Plutus Module

Plutus ingests, aggregates, analyzes, and acts on market data for FFXIV, OSRS, and US
equities: trades, signals, forecasts, strategies with backtests and optimization, and the
market and symbol analyst assistants.

This is the module's arc42 entry point, refining the platform's building block view
([platform Section 5](../../../../docs/architecture/05-building-block-view.md)) one level
down. Platform-wide concepts (module contract, messaging, persistence, assistants) are
described in the [platform documentation](../../../../docs/architecture/README.md) and are
not repeated here.

## Contents

| Section | Focus |
|---------|-------|
| [5. Building Block View](05-building-block-view.md) | The module's feature slices and domain folders |
| [6. Runtime View](06-runtime-view.md) | Trade ingestion, signals and forecasting, analyst assistants |
| [6. Backtest and Optimization](runtime/backtest-optimization.md) | The backtest lifecycle, simulators, optimizer, recommendations |
| [8. Concepts](08-concepts.md) | Module assistants and their tools, caching, WebSocket hosting |
| [11. Risks and Technical Debt](11-risks-and-technical-debt.md) | Module-scoped debt (`PLUTUS-D*`) |
| [12. Glossary](12-glossary.md) | Plutus ubiquitous language |
| [Decisions](adr/README.md) | Module ADR log (`PLUTUS-ADR-*`) |

## Responsibilities

- Ingest trades from external market feeds and persist them in TimescaleDB hypertables with
  continuous aggregates for query-ready market views
- Compute signals every five minutes and daily ML price forecasts
- Manage markets, symbols, symbol groups, crafting recipes and user-recorded positions
- Define strategies, run backtests and genetic-algorithm optimizations, and produce live
  recommendations
- Offer page-scoped AI analysts for the market, Explorer and symbol pages

## Context

| Neighbour | Channel | Purpose | Code entry point |
|-----------|---------|---------|------------------|
| Universalis | Outbound WSS (`wss://universalis.app/api/ws`, BSON frames) | Real-time FFXIV sale events | `Features/DataLoaders/Ffxiv/` |
| XIVAPI item data | Outbound HTTPS (CSV on `raw.githubusercontent.com`, fetched lazily and cached) | FFXIV item names for incoming sale events | `Features/DataLoaders/Ffxiv/XivApi/` |
| OSRS Wiki API | Outbound HTTPS (JSON, polled every 5 minutes) | OSRS prices and item mappings | `Features/DataLoaders/Osrs/OsrsWikiClient.cs` |
| Alpaca (IEX feed) | Outbound WSS (`wss://stream.data.alpaca.markets/v2/iex`, JSON) | Real-time US equity trades | `Features/DataLoaders/Stocks/` |
| OuranosMl | Outbound HTTP (`POST /plutus/forecast`; Responses API for the analysts) | Price forecasting (`ForecastModelClient`) and analyst assistants | `Features/Forecasts/`, `Features/Assistants/` |
| Interface | Inbound REST + SSE | Plutus pages under `src/apps/interface/src/app/plutus/` | Every `*Endpoint.cs` |

Code paths are relative to `Ouranos.Pantheon.Modules.Plutus/`. Plutus depends only on the
shared kernel; it neither calls nor is called by another module.

## Provided Interfaces

- **REST**: routes under `/api/plutus/...`, one Bruno request per endpoint in
  `bruno/ouranos-pantheon/collections/API/Plutus/`. Backtest, optimize and restart return
  `202 Accepted` and are polled.
- **SSE**: `/api/plutus/symbols/assistant/completions/stream` (Symbol Analyst) and
  `/api/plutus/markets/assistant/completions/stream` (Market Analyst).

### Messages

All are module-internal; none crosses a module boundary.

| Message | Exchange | Queue | DLQ |
|---------|----------|-------|-----|
| `TradeMessage` | `plutus.trade` | `plutus.trade.ingest` | `plutus.trade.ingest.dlq` |
| `RunBacktestMessage` | `plutus.backtest` | `plutus.backtest.run` | `plutus.backtest.run.dlq` |
| `OptimizeStrategyMessage` | `plutus.backtest` | `plutus.backtest.optimize` | `plutus.backtest.optimize.dlq` |

The backtest and optimization consumers record failures on the `Backtest` instead of
throwing, so their DLQs only see infrastructure faults.

### Scheduled Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|
| `OsrsDataLoaderJob` | every 5 min | Poll OSRS Wiki prices → publish `TradeMessage`s |
| `SymbolSignalCalculate` | every 5 min | Recompute signals for all symbols and refresh `plutus.latest_signals` |
| `ForecastGenerator` | daily | Generate price forecasts via OuranosMl |

## Persistence

Schema `plutus` (EF Core, `PlutusDbContext`). Trades, signals and forecasts are TimescaleDB
hypertables; continuous aggregates back the market views, and `plutus.latest_signals` is a
plain materialized view refreshed by the signal job. Strategy configurations are one
polymorphic `jsonb` column keyed by mode.

## Configuration

All settings bind from `Ouranos:Plutus` (`PlutusOptions`):

| Section | Purpose |
|---------|---------|
| `DataLoaders` | Enable flags per loader (`Ffxiv`, `Osrs`, `Stocks`, `Consumer`; all default to enabled) and loader settings |
| `Forecasting` | Forecast enable flag and model settings |
| `Signals` | Signal computation settings |
| `BacktestData`, `FlipData` | Simulator data settings (e.g. `LimitWindowsPerDay`) |
| `Optimization` | Genetic-algorithm defaults |
| `SymbolAnalyst`, `MarketAnalyst` | Assistant prompt and model overrides (e.g. `ContextCacheMinutes`) |
| `AssistantTools` | Row and chart limits shared by the analyst tools (`ChartPoints`, `MaxPositions`, …) |

## Quality Notes

- Hot dashboard aggregates (market overview, volume heatmap) and strategy recommendations
  use server-side output caching ([ADR 0010](../../../../docs/adr/0010-server-side-output-caching-for-dashboard-aggregates.md)).
- Backtest results describe trades that could actually have been placed: no lookahead,
  liquidity-capped fills and per-unit market tax (see
  [Backtest and Optimization](runtime/backtest-optimization.md)).
