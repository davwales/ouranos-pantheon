# 11. Risks and Technical Debt: Plutus

Module-scoped items. Platform-wide risks stay in
[platform Section 11](../../../../docs/architecture/11-risks-and-technical-debt.md); the
former platform IDs are noted so older references still resolve.

| # | Item | Where | Risk / impact | Proposed severity |
|---|------|-------|---------------|-------------------|
| PLUTUS-D1 (was D7) | **No fast path to realistic local Plutus data**: data-heavy local work (signals, backtests) rides on full prod restores (large `pg_dump`s, slow dump/restore cycles); faking data is unexplored and risks diverging from real market distributions | Local development data flow | Slow iteration on data-heavy features; synthetic-data shortcuts could mislead signal/backtest tuning | Medium |
| PLUTUS-D2 (was D10, Plutus part) | **Cross-slice type imports**: strategy optimization and recommendations, and signal history, import another slice's schemas or pipeline steps | `Features/Strategies`, `Features/Signals` | Slices are no longer independently changeable; shared types should move to the module's `Shared/` folder | Low |
| PLUTUS-D3 (was D14) | **Dependence on free external APIs**: Universalis, OSRS Wiki prices API, Alpaca free IEX feed, XIVAPI item data from GitHub | `Features/DataLoaders` | Rate limits, terms changes or outages stop ingestion with no fallback; IEX-only volume understates real activity for stock signals | Medium |
| PLUTUS-D4 (was D16) | **Ineffective overlap guard**: `SymbolSignalCalculateJob` uses an `Interlocked` instance field, but TickerQ creates a new job instance per run, so the flag never sees a concurrent run | `Features/Signals/SymbolSignalCalculate` | If a run exceeds its 5-minute interval, runs may overlap, duplicating signal writes and view refreshes | Medium |
| PLUTUS-D5 (was D19) | **Magic strings in query inputs**: assistant tools and their prompts build list-query inputs from raw strings: sort directions (`"desc"`/`"asc"`), filter expressions (`$"SymbolId:eq:{id}"`, `$"Name:like:{name}"`), sort fields (`"DayOne.Gain"`, camel-cased column ids) and default-order text (`"totalGain desc"`). Only some of these use `nameof` | `Features/Assistants/Tools` (`MarketTradesFilter`, tools), `Features/Assistants/AskSymbolAnalyst`, `Features/Assistants/AskMarketAnalyst`; the shared filter/sort syntax behind `QueryOptions` | Renaming a response field or filter operator breaks queries at runtime, not at compile time, and unit tests that match the same literals stay green. The backend should expose typed constants or builders for field names, operators and sort directions | Low |
| PLUTUS-D6 | **No way to delete a market with data**: `DELETE /api/plutus/markets/{id}` returns 409 while the market has symbols, because the cascade would cover hundreds of millions of rows in compressed trade, signal and forecast chunks in one transaction | `Features/Markets/DeleteMarket` | Retiring a market needs a manual, batched data removal (and its `Consumer:MarketMap` entry removed); automating it needs a background, batched deletion job | Low |

Plutus jobs and caches also carry the platform's single-instance assumption (platform D12).
