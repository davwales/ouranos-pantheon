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

## Plutus

| Term | Definition |
|------|------------|
| **Market** | A market the platform tracks (FFXIV, OSRS, US equities), with its sale taxes and a per-market forecasting switch |
| **Symbol** | A tradable item/ticker within a market (an FFXIV item, an OSRS item, a stock ticker), identified by `Code` plus optional `Subcode` |
| **Subcode** | Variant qualifier on a symbol: `hq`/`nq` for FFXIV high/normal quality, `p2p`/`f2p` for OSRS members/free-to-play items; unset for stocks |
| **Trade** | A price/volume/timestamp row for a symbol. FFXIV and stock trades are individual sales from the WebSocket feeds; OSRS trades are 5-minute average price/volume buckets from the Wiki prices API |
| **Producer** | The data source tag on a `TradeMessage` (`Ffxiv`, `Osrs`, `Stocks`); the consumer maps it to a market |
| **Data loader** | An ingestion component: WebSocket listeners (FFXIV, Alpaca), the OSRS poller, and the trade consumer that persists `TradeMessage`s; each can be toggled in configuration |
| **Taxes** | Per-market sale tax model; today a `FlatTax` (rate with minimum and maximum), e.g. the OSRS Grand Exchange 2% capped tax. Used by tax-adjusted ROI and backtests |
| **Time frame** | The window a trade query or snapshot covers, from minutes up to unbounded (`AllTime`) |
| **Market overview** | Bucketed price/volume time series for a market |
| **Volume heatmap** | Share of a market's trades by day of week and hour |
| **Signal** | A computed analytical indicator on a symbol (RSI, Bollinger Bands, Moving Average Crossover, Trend Momentum, Volume Anomaly, Tax-Adjusted ROI, Price Velocity), recalculated every 5 minutes |
| **Investment intent** | Flags describing what a signal supports: `Buy`, `Sell`, `Flip` (quick resale), `Merch` (merchanting, buy low and hold to sell high) |
| **Strategy** | A per-market decision rule set: input weights over signal kinds, buy/sell thresholds, and position-sizing limits |
| **Backtest** | Historical simulation of a strategy over a market, period and budget, producing metrics and simulated positions. Runs asynchronously and can be cancelled or restarted; optimization runs are stored as backtests too |
| **Strategy optimization** | Genetic-algorithm search over a strategy's input weights, scored by backtest outcomes on in-sample and out-of-sample periods |
| **Fitness weights** | The optimization objective's weights: Sortino, CAGR, drawdown, turnover, and L1 regularization |
| **Recommendation** | Output of a strategy for a budget: scored symbols with suggested allocation, volume, current price and rationale |
| **Position** | A user-recorded buy or sell for a symbol with cost and quantity; a sell can be linked to its buy |
| **Forecast** | ML-generated price prediction for a symbol from OuranosMl, produced daily for markets with forecasting enabled |
| **Forecast efficacy** | Accuracy of past forecast records against realized trades, computed on demand |
| **Symbol group** | A user-defined, per-market named set of symbols for browsing in the UI |
| **Recipe (Plutus)** | A crafting recipe in a game market: input and output `RecipeComponent`s (symbol + quantity) and a fixed `Cost`, used for crafting-margin analysis |

## Hermes

| Term | Definition |
|------|------------|
| **Persona** | A reusable assistant personality (name, description, personality, scenario) selected when a conversation is created |
| **Model config** | A named configuration of an available model (system prompt and sampling settings); marked unavailable when the model disappears from OuranosMl |
| **Trait** | A named, reusable system-prompt fragment attached to a conversation and merged with the persona and model config when the system prompt is built |
| **Conversation** | A chat thread with an LLM using one persona and model config; tracks token usage and may sit in a folder |
| **Message** | An individual message within a conversation (system, user, assistant or summary role) |
| **Folder** | Nestable container for conversations (`ParentFolderId`) |
| **Compaction** | Summarizing a conversation's messages since the last summary into a single `Summary`-role message so long chats fit the context window; streamed over SSE |
| **Available model** | An LLM served by OuranosMl, discovered by the hourly sync job |
| **Default** | `IsDefault` on a persona or model config: the one preselected for new conversations |
| **Public mode** | Flagsmith flag `hermes.public_mode`: when on, Hermes read endpoints return only items marked `IsPublic` |

## Hestia

| Term | Definition |
|------|------------|
| **Recipe** | A cooking recipe with full event-sourced version history (ingredients, steps, notes) |
| **Recipe version** | A historical state of a recipe (event-sourced) |
| **Revert** | Restoring a prior recipe version as the current state |
| **Import** | Async pipeline turning a recipe-website URL into structured recipe data (scrape the page's JSON-LD recipe → LLM normalize → persist) |
| **Import status** | Where a recipe's import stands: none, importing, imported, or failed |
| **Re-import** | Running the import pipeline again for an existing recipe |
| **Shopping list** | A single Marten document combining ingredients of the recipes toggled onto it with manual items |
| **Manual item** | A free-text entry added to the shopping list directly rather than from a recipe |

> **Naming collision note:** *Recipe* appears in both Plutus (crafting-recipe **cost
> analysis** for game economies) and Hestia (recipe **management**). They are unrelated
> features in different modules. Context determines which is meant.

## Shared

| Term | Definition |
|------|------------|
| **Notification** | Outbox row for a message to send on a channel (`Discord`, `Email`, `Desktop`) |
| **Health check** | An implementation of the Shared module's own `IHealthCheck`; results are aggregated by `GET /health` |

## Technical

| Term | Definition |
|------|------------|
| **Wolverine** | .NET message-handling framework: in-process handler dispatch + RabbitMQ transport |
| **Marten** | Event-sourcing + document database library running on PostgreSQL (used by Hestia) |
| **TickerQ** | In-process scheduler with EF Core store and dashboard (all recurring jobs) |
| **Hypertable** | TimescaleDB table partitioned by time, used for trades, signals, and forecasts |
| **Continuous aggregate (cagg)** | TimescaleDB materialized view maintained incrementally over a hypertable |
| **Output cache** | ASP.NET Core server-side HTTP response cache, enabled per endpoint on hot Plutus reads |
| **Step pipeline** | `IStep`/`StepRegistry` chain of responsibility in the kernel; composes backtest execution |
| **Genetic algorithm** | Generic evolutionary search in the kernel (`Algorithms/Genetic`), used by strategy optimization |
| **OuranosMl** | Self-hosted ML inference service exposing an OpenAI-compatible API and a custom forecasting endpoint |
| **Flagsmith** | Feature-flag service |
| **Universalis** | Community FFXIV market data provider |
| **Alpaca** | Market data provider for US equities (WebSocket trade stream, free IEX feed) |
| **XIVAPI** | FFXIV game data source; the item list is read from a static GitHub-hosted CSV and cached |
| **ADR** | Architecture Decision Record |
| **MADR** | Markdown Architecture Decision Record, the lightweight ADR template used in [`docs/adr/`](../adr/README.md) |
| **arc42** | The architecture documentation template this documentation follows |

## Abbreviations

| Abbreviation | Meaning |
|--------------|---------|
| CAGR | Compound annual growth rate |
| CORS | Cross-Origin Resource Sharing |
| DDD | Domain-Driven Design |
| DLQ | Dead-letter queue: the `.dlq`-suffixed queue receiving messages after retries are exhausted |
| F2P / P2P | Free-to-play / pay-to-play (OSRS members) |
| FFXIV | Final Fantasy XIV |
| GA | Genetic algorithm |
| GHCR | GitHub Container Registry, where CI publishes the gateway and interface images |
| HQ / NQ | High quality / normal quality (FFXIV items) |
| IEX | Investors Exchange; Alpaca's free real-time feed covers IEX trades only |
| LLM | Large language model |
| ML | Machine learning |
| OSRS | Old School RuneScape |
| OTLP | OpenTelemetry Protocol, used to export traces and metrics |
| ROI | Return on investment |
| RSI | Relative Strength Index |
| SSE | Server-Sent Events, used for Hermes streaming responses |
| SSRF | Server-side request forgery; the recipe scraper uses an anti-SSRF HTTP handler |
| VSA | Vertical Slice Architecture |
| WSS | WebSocket Secure |
