# Plutus

## Domain Overview

The Plutus service is responsible for displaying and aggregating market data from a variety of sources. These sources at
time of writing include Oldschool RuneScape, Final Fantasy XIV, and the Stock Market. Data is ingested asynchronously
through a data-loader module.

The goal of this service is to allow users to easily identify trends or discrepancies in market data that will allow
them to make informed decisions in what to invest in for profit.

## Usage

This module is hosted via the gateway. If you wish to run this yourself, you will need to run the gateway or otherwise
create your own.

You will need to configure the following environment variables, however:

- Ouranos:Mongo:ConnectionString
- Ouranos:OuranosMl:ConnectionString

It is highly recommended that you set the following variable to `false` for local development.

- Ouranos:Plutus:Forecasting:IsEnabled

## Architecture

```
plutus/
  ├── API/
  ├── Application/
  ├── Domain/
  │   ├── Forecasts/
  │   ├── Markets/
  │   ├── Recipes/
  │   ├── Symbols/
  │   ├── Trades/
  └── Infra.*/
```

## Dependencies

- GraphQL
- MongoDB
- Ouranos.Pantheon.Core.API
- Ouranos.Pantheon.Core.Application
- Ouranos.Pantheon.Core.Domain
- Ouranos.Pantheon.Core.Infra.Mongo
- Ouranos.Pantheon.Core.Infra.OuranosMl

## Database

- plutus
    - markets
    - symbols
    - trades
    - recipes
    - forecasts
