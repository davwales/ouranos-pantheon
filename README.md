# Ouranos Pantheon

## Architecture Overview

- Clean Architecture
- Domain-driven design
- Module isolation through explicit boundaries (modular monolith)

## Module Structure

```
src/
  ├── core/
  ├── data-loaders/
  │   ├── module-a/
  │   │   ├── consumer/
  │   │   ├── producer-a/
  │   │   └── producer-b/
  │   └── module-b/
  ├── gateway/
  └── services/
      ├── module-a/
      │   ├── API/
      │   ├── Application/
      │   ├── Domain/
      │   └── Infra.*/
      ├── module-b/
      └── module-c/
tests/
```

## Technology Stack

- .NET 8
- GraphQL
- MongoDB
- RabbitMQ
- MassTransit

## Getting Started

1. Prerequisites
    1. Infrastructure Deployed (recommend using the docker compose defined in the public ouranos-docker-infrastructure
       repository)
        1. MongoDB
        2. RabbitMQ
        3. OuranosML
2. Configuration
    1. Override necessary configuration to connect to deployed infrastructure.
3. Database Setup
    1. Recommended to setup the "plutus.trades" collection as a time series with the following indexes.
        1. metadata.messageId
        2. metadata.symbolId
        3. metadata.marketId
        4. metadata.marketId_createdAt
4. Running the Application
    1. Run the Pantheon Gateway or the desired producer/consumer data loader.

## Contributing

At this point I am not interested in additional contributors. This project is mostly just a learning tool for myself and
showcase of my development practices in a practical application that makes my life easier or involves a technology I
want to have experience with.
