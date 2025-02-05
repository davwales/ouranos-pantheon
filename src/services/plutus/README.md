# Plutus

## Domain Overview

The Plutus service is responsible for displaying and aggregating market data from a variety of sources. These sources at
time of writing include Oldschool RuneScape, Final Fantasy XIV, and the Stock Market. Data is ingested asynchronously
through a data-loader module.

The goal of this service is to allow users to easily identify trends or discrepancies in market data that will allow
them to make informed decisions in what to invest in for profit.

## Architecture

```
plutus/
  ├── API/
  ├── Application/
  ├── Domain/
  │   ├── Characters/
  │   ├── Conversations/
  └── Infra.Mongo/
```

## Dependencies

- GraphQL
- MongoDB
- Ouranos.Pantheon.Core.API
- Ouranos.Pantheon.Core.Application
- Ouranos.Pantheon.Core.Domain
- Ouranos.Pantheon.Core.Mongo

## Module API

- Queries
    - Markets
        - GetMarket
        - GetAllMarkets
    - Symbols
        - GetSymbol
        - GetAllSymbols
    - Trades
        - GetMarketTrades
        - GetSymbolTrades
- Mutations
    - Markets
        - CreateMarket
        - UpdateMarket
        - DeleteMarket

## Database

- plutus
    - markets
    - symbols
    - trades
