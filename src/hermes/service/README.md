# Hermes

## Domain Overview

The Hermes service allows users to create characters and then have conversations between them. Ideally this is a
showcase of how one could implement interactions with a self-hosted LLM.

## Usage

This module is hosted via the gateway. If you wish to run this yourself, you will need to run the gateway or otherwise
create your own.

You will need to configure the following environment variables, however:

- Ouranos:Mongo:ConnectionString
- Ouranos:OuranosMl:ConnectionString
- Ouranos:Hermes:OuranosMl:SystemPrompt

## Architecture

```
hermes/
  ├── API/
  ├── Application/
  ├── Domain/
  │   ├── Characters/
  │   ├── Conversations/
  └── Infra.OuranosMl/
```

## Dependencies

- GraphQL
- MongoDB
- Ouranos.Pantheon.Core.API
- Ouranos.Pantheon.Core.Application
- Ouranos.Pantheon.Core.Domain
- Ouranos.Pantheon.Core.Mongo
- Ouranos.Pantheon.Core.OuranosMl

## Database

- hermes
    - characters
