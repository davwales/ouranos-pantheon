# 5. Building Block View: Plutus

Level-2 whitebox of the Plutus blackbox described in the [module README](README.md). Paths
are relative to `Ouranos.Pantheon.Modules.Plutus/`.

| Building block | Responsibility |
|----------------|----------------|
| `Features/DataLoaders/` | Ingestion: FFXIV and Stocks WebSocket listeners, OSRS poller job, and a cached XIVAPI item lookup; the trade consumer upserts symbols and writes trades |
| `Features/Assistants/` | Everything assistant-related: one folder per assistant (below), the tool catalogue in `Tools/` (one folder per tool, with the queries and shaping they share), and in `Scopes/` the context interfaces tools are typed to (`IMarketScope`, `ISymbolScope`, `IExplorerScope`) |
| `Features/Assistants/AskMarketAnalyst/` | Streaming Market Analyst (`PantheonAssistant<MarketAnalystContext>`) for the market landing and Explorer pages: composes market taxes, the market overview and the user's current Explorer view into a compact prompt; opts into catalogue tools that screen the market, look up symbols, rank by signals or forecasts, summarize trading hours and set the Explorer view |
| `Features/Assistants/AskSymbolAnalyst/` | Streaming Symbol Analyst (`PantheonAssistant<SymbolAnalystContext>`): composes a lean headline of symbol, market and today's prices from existing query handlers; opts into catalogue tools that fetch price history, signals, the forecast, positions, and find and look up other symbols on demand |
| `Features/Markets/` | Market catalog CRUD; markets are the root entity the rest of the module is scoped to |
| `Features/Symbols/` | Symbol read access: list/get symbols (upserted by ingestion) and today's per-symbol summary statistics |
| `Features/Trades/` | Trade queries and aggregate views over the TimescaleDB hypertable and continuous aggregates: all trades, per-symbol/market/recipe trade aggregates, market overview, volume heatmap |
| `Features/Signals/` | Scheduled signal computation via pluggable signal computers covering technical and game-economy indicators, plus signal queries; scheduling in the [runtime view](06-runtime-view.md) |
| `Features/Strategies/` | Mode-agnostic strategy slices: strategy CRUD, recommendations, and the backtest lifecycle (run, optimize, get, cancel, restart) plus a cross-strategy list of pending and running backtests. `BacktestJobRunner` owns every status transition of a queued run; `StrategyOptimizer` is the one genetic-algorithm flow for every mode |
| `Shared/Domain/Strategies/` | The strategy engines. Each `StrategyMode` (`Swing/`, `Flip/`) is one folder with its configuration, data loader, a pure simulator and an `IStrategyEngine` (load, simulate, parameter space, recommend); `Trading/` holds what the simulators share (holdings, equity curve, market tax rules, result metrics) |
| `Features/Forecasts/` | Daily ML price forecasts via OuranosMl, forecast queries, and on-demand forecast efficacy |
| `Features/Positions/` | CRUD for user-recorded positions, including closing and linking a sell to its buy |
| `Features/SymbolGroups/` | CRUD for named groupings of symbols within a market |
| `Features/Recipes/` | CRUD for crafting recipes (input/output components, cost) that back the per-recipe trade aggregation and cost analysis |
| `Shared/Domain/` | Market, Symbol, SymbolGroup, Trade, Recipe, Signal, Strategy, Backtest, Position, Forecast entities |

Within the module, coupling between slices is data coupling: the trade and backtest
handoffs travel over RabbitMQ (see [Messages](README.md#messages)), and everything else is
one slice reading what another has written to the `plutus` schema.
