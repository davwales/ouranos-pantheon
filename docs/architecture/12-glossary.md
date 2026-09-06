# 12. Glossary

## Platform

| Term | Definition |
|------|------------|
| **Pantheon** | The platform as a whole: gateway + interface + modules |
| **Module** | A self-contained domain unit implementing `IPantheonModule`; references only the shared kernel |
| **Gateway** | The .NET host that composes all modules into one deployable REST API |
| **Interface** | The Next.js dashboard application |
| **Shared kernel** | Abstractions and generic infrastructure referenced by every module (`Ouranos.Pantheon.Modules.Shared.Contract`) |
| **Vertical slice** | A feature folder containing handler, endpoint, and schemas, organized end-to-end rather than by technical layer |

## Plutus

| Term | Definition |
|------|------------|
| **Market** | A market the platform tracks: FFXIV, OSRS, or US equities |
| **Symbol** | A tradable item/ticker within a market (e.g. an FFXIV item id, a stock ticker) |
| **Trade** | A single executed transaction ingested from a data provider |
| **Data loader** | An ingestion component: WebSocket listeners (FFXIV, Alpaca), the OSRS poller, and the XIVAPI item sync |
| **Signal** | A computed analytical indicator on a symbol (RSI, Bollinger Bands, Moving Average Crossover, Trend Momentum, Volume Anomaly, Tax-Adjusted ROI, Price Velocity) |
| **Strategy** | A configured decision rule set: input weights over signal kinds plus buy/sell thresholds |
| **Backtest** | Historical simulation of a strategy over a market and period, producing metrics |
| **Strategy Optimization** | Genetic-algorithm search over strategy input weights against objectives (Sortino ratio, CAGR, drawdown, turnover) against backtest outcomes |
| **Position** | An open or recommended position derived from signals and strategy configuration |
| **Forecast** | ML-generated price prediction from, evaluated for efficacy against realized trades |
| **Symbol group** | Named grouping of symbols (e.g. crafting chains) for aggregate analysis |

## Hermes

| Term | Definition |
|------|------------|
| **Persona** | A reusable assistant personality (name, description, personality, scenario) selected when a conversation is created |
| **Trait** | A named, reusable system-prompt fragment attached to a conversation and merged with the persona when the system prompt is built |
| **Conversation** | A chat thread with an LLM |
| **Message** | An individual message within a conversation |
| **Available model** | An LLM served by OuranosMl, discovered by the hourly sync job |

## Hestia

| Term | Definition |
|------|------------|
| **Recipe** | A cooking recipe with full event-sourced version history (ingredients, steps, notes) |
| **Recipe version** | A historical state of a recipe (event-sourced) |
| **Revert** | Restoring a prior recipe version as the current state |
| **Import** | Async pipeline turning a recipe-website URL into structured recipe data (scrape → LLM normalize → persist) |
| **Shopping list** | Aggregated ingredient list derived from recipes or manually added items |

> **Naming collision note:** *Recipe* appears in both Plutus (crafting-recipe **cost
> analysis** for game economies) and Hestia (recipe **management**). They are unrelated
> features in different modules. Context determines which is meant.

## Technical

| Term | Definition |
|------|------------|
| **Wolverine** | .NET message-handling framework: in-process handler dispatch + RabbitMQ transport |
| **Marten** | Event-sourcing + document database library running on PostgreSQL (used by Hestia) |
| **TickerQ** | In-process scheduler with EF Core store and dashboard (all recurring jobs) |
| **Hypertable** | TimescaleDB table partitioned by time, used for trades, signals, and forecasts |
| **Continuous aggregate (cagg)** | TimescaleDB materialized view maintained incrementally over a hypertable |
| **DLQ** | Dead-letter queue: the `.dlq`-suffixed queue receiving messages after retries are exhausted |
| **OuranosMl** | Self-hosted ML inference service exposing an OpenAI-compatible API and a forecasting endpoint |
| **Flagsmith** | Feature-flag service |
| **Universalis** | Community FFXIV market data provider |
| **Alpaca** | Market data provider for US equities (WebSocket trade stream) |
| **XIVAPI** | FFXIV game data source; item data is synced from a static GitHub-hosted dump |
| **ADR** | Architecture Decision Record |
| **MADR** | Markdown Architecture Decision Record, the lightweight ADR template used in [`docs/adr/`](../adr/README.md) |
| **arc42** | The architecture documentation template this documentation follows |

## Abbreviations

| Abbreviation | Meaning |
|--------------|---------|
| DLQ | Dead-letter queue |
| FFXIV | Final Fantasy XIV |
| GA | Genetic algorithm |
| OSRS | Old School RuneScape |
| VSA | Vertical Slice Architecture |
| WSS | WebSocket Secure |
