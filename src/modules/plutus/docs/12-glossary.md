# 12. Glossary: Plutus

The ubiquitous language of the Plutus bounded context. Platform and technical terms are in
the [platform glossary](../../../../docs/architecture/12-glossary.md).

| Term | Definition |
|------|------------|
| **Market** | A market the platform tracks (FFXIV, OSRS, US equities), with its sale taxes and a per-market forecasting switch |
| **Symbol** | A tradable item/ticker within a market (an FFXIV item, an OSRS item, a stock ticker), identified by `Code` plus optional `Subcode` |
| **Subcode** | Variant qualifier on a symbol: `hq`/`nq` for FFXIV high/normal quality, `p2p`/`f2p` for OSRS members/free-to-play items; unset for stocks |
| **Trade** | A price/volume/timestamp row for a symbol. FFXIV and stock trades are individual sales from the WebSocket feeds; OSRS trades are 5-minute average price/volume buckets from the Wiki prices API |
| **Producer** | The data source tag on a `TradeMessage` (`Ffxiv`, `Osrs`, `Stocks`); the consumer maps it to a market |
| **Data loader** | An ingestion component: WebSocket listeners (FFXIV, Alpaca), the OSRS poller, and the trade consumer that persists `TradeMessage`s; each can be toggled in configuration |
| **Taxes** | Per-market sale tax model; today a `FlatTax` (rate with minimum and maximum), e.g. the OSRS Grand Exchange 2% capped tax. Charged per unit by one rule everywhere: the rate applied to the price, rounded down, waived below the minimum and capped at the maximum. C# goes through `MarketRules.UnitTax` (simulators, recommendations, tax-adjusted ROI, forecasts); SQL goes through `plutus.unit_tax` (the `market_trade_snapshots` tax read by the explorer and symbol groups) |
| **Time frame** | The window a trade query or snapshot covers, from minutes up to unbounded (`AllTime`) |
| **Market overview** | Bucketed price/volume time series for a market |
| **Volume heatmap** | Share of a market's trades by day of week and hour |
| **Signal** | A computed analytical indicator on a symbol (RSI, Bollinger Bands, Moving Average Crossover, Trend Momentum, Volume Anomaly, Tax-Adjusted ROI, Price Velocity), recalculated every 5 minutes |
| **Investment intent** | Flags describing what a signal supports: `Buy`, `Sell`, `Flip` (quick resale), `Merch` (merchanting, buy low and hold to sell high) |
| **Strategy** | A per-market decision rule set whose configuration is one of the strategy modes. Swing strategies hold input weights over signal kinds, buy/sell thresholds and position-sizing limits; flip strategies hold flip settings |
| **Strategy engine** | The `IStrategyEngine` for one strategy mode: loads market history, simulates a configuration, describes its tunable parameters and produces recommendations |
| **Flip strategy** | A strategy that repeatedly buys at the recent sell-side price and sells at the recent buy-side price, re-placing orders every few hours within a limit on open orders and each symbol's buy limit |
| **Backtest** | Historical simulation of a strategy over a market, period and budget, producing metrics and simulated positions. Runs asynchronously and can be cancelled or restarted; optimization runs are stored as backtests too |
| **Strategy optimization** | Genetic-algorithm search over a swing strategy's own input weights (signed), thresholds and sizing, or a flip strategy's quoting and sizing settings, scored on in-sample segments (~30 days for swing, 7 for flip) and validated on a held-out out-of-sample period. Locked parameters are kept at their configured values |
| **Fitness weights** | The optimization objective's weights: Sortino, CAGR, drawdown, turnover, and L1 regularization |
| **Recommendation** | What a strategy's simulator would buy right now for a budget: symbols with score, suggested allocation, volume, price and rationale, plus buy/sell prices and expected profit for flip strategies |
| **Position** | A user-recorded buy or sell for a symbol with cost and quantity; a sell can be linked to a Bought buy of the same market, symbol and strategy, up to the buy's unsold quantity |
| **Forecast** | ML-generated price prediction for a symbol from OuranosMl, produced daily for markets with forecasting enabled |
| **Forecast efficacy** | Accuracy of past forecast records against realized trades, computed on demand |
| **Symbol group** | A user-defined, per-market named set of symbols for browsing in the UI |
| **Recipe** | A crafting recipe in a game market: input and output `RecipeComponent`s (symbol + quantity) and a fixed `Cost`, used for crafting-margin analysis. Unrelated to Hestia's cooking recipes |
| **Universalis** | Community FFXIV market data provider |
| **Alpaca** | Market data provider for US equities (WebSocket trade stream, free IEX feed) |
| **XIVAPI** | FFXIV game data source; the item list is read from a static GitHub-hosted CSV and cached |

## Abbreviations

| Abbreviation | Meaning |
|--------------|---------|
| CAGR | Compound annual growth rate |
| F2P / P2P | Free-to-play / pay-to-play (OSRS members) |
| FFXIV | Final Fantasy XIV |
| HQ / NQ | High quality / normal quality (FFXIV items) |
| IEX | Investors Exchange; Alpaca's free real-time feed covers IEX trades only |
| OSRS | Old School RuneScape |
| ROI | Return on investment |
| RSI | Relative Strength Index |
| VWAP | Volume-weighted average price |
