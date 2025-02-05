# Hermes

## Domain Overview

The Hermes service allows users to create characters and then have conversations between them. Ideally this is a
showcase of how one could implement interactions with a self-hosted LLM.

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

## Module API

- Queries
    - Character
        - GetCharacter
        - GetAllCharacters
- Mutations
    - Character
        - CreateCharacter
        - UpdateCharacter
        - DeleteCharacter
    - Conversation
        - GenerateCompletion

## Database

- hermes
    - characters
