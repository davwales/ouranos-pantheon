# 8. Concepts: Plutus

Module-specific applications of the platform's
[crosscutting concepts](../../../../docs/architecture/08-crosscutting-concepts.md).

## 8.1 Assistants

Plutus follows the platform assistant pattern (framework in the
[Shared module docs](../../shared/docs/README.md)). Tools live in the catalogue under
`Features/Assistants/Tools/` and are typed to the scopes `IMarketScope`, `ISymbolScope` and
`IExplorerScope` (`Features/Assistants/Scopes/`).

| Assistant | Route | Context | Tools |
|-----------|-------|---------|-------|
| `SymbolAnalyst` | `/api/plutus/symbols/assistant/completions/stream` | `{ marketId, symbolId, timeFrame }`; the symbol must belong to the market | Read: `get_price_history`, `get_signals`, `get_forecast`, `get_positions` (all about the page's symbol), `find_symbols` and `get_symbol` (any other symbol in the market, by an id `find_symbols` returns) |
| `MarketAnalyst` | `/api/plutus/markets/assistant/completions/stream` | `{ marketId, timeFrame, view? }`, where `view` carries the Explorer's filter, sort and page | Read: `find_symbols`, `get_signal_rankings`, `get_forecast_leaders` (each returning symbol ids), `get_symbol` (by one of those ids), `get_activity_hours`. On the Explorer also `set_explorer_view`, which proposes one view per reply for the user to apply |

The analysts are configured by `Ouranos:Plutus:SymbolAnalyst` and
`Ouranos:Plutus:MarketAnalyst`. Both use a reduced output budget to fit the limited context
window, and the tools' row and chart limits are shared under `Ouranos:Plutus:AssistantTools`.
How each analyst composes its prompt is in the [runtime view](06-runtime-view.md).

## 8.2 Output Caching

The market overview and volume heatmap endpoints are cached with the platform's output-cache
mechanism, which was introduced for them
([ADR 0010](../../../../docs/adr/0010-server-side-output-caching-for-dashboard-aggregates.md)).
Strategy recommendations are cached for 5 minutes and tagged per strategy, so updating or
deleting a strategy evicts only its own entries.

## 8.3 WebSocket Feeds

Plutus hosts the kernel's `WebSocketWorker` twice (FFXIV, Stocks) via factory methods in
`PlutusModule.Build`. Connection state is surfaced to the platform health check through
`WebSocketHealthState`. The Universalis feed is BSON; `MongoDB.Bson` is used as a parser only
(`Features/DataLoaders/Ffxiv/Serializers/BsonMessageConverter.cs`), and no MongoDB server is
involved.

## 8.4 Strategy Configuration

A strategy's configuration is polymorphic by `mode` (`Swing`, `Flip`) and stored as one
`jsonb` column via `HasJsonConversion`, in the same camelCase, string-enum shape the API
uses, so adding a field needs no migration.
