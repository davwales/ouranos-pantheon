# Pantheon Gateway

## Domain Overview

The role of the gateway is to aggregate all desired Pantheon dependencies to be hosted and thus deployed as a singular
Pantheon application. This allows various modules to define their own APIs and then the gateway references and combines
them into a singular interface.

## Dependencies

- GraphQL
- Plutus API
- Hermes API

## API

You can access StrawberryShake for the HotChocolate GraphQL server at http://localhost:8300/graphql/.