# Plutus Data Loaders

## Domain Overview

The intent of this module is to ingest data from various providers and then standardize the processing of it in such a
way that they can enjoy seamless implementation of features within the Plutus service.

To achieve this, you will note a few submodules, one for each provider as well as one for the consumer. You will note
that there are some common data loading projects that the submodules use to standardize their communication with the
message queue. At time of writing producers follow one of two patterns: listening for web socket communication or
periodically retrieving data from an external API.

## Architecture

```
plutus/
  ├── consumer/
  ├── ffxiv/
  ├── osrs/
  ├── stocks/
  ├── Application
  ├── Domain
  ├── RabbitMq
  └── Worker
```

## Dependencies

- Plutus Service Domain
- MongoDB
- RabbitMQ
- Universalis WebSocket
- XivApi
- OSRS Wiki
- Alpaca

## Database

- plutus
