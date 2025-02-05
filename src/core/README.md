# Pantheon Core

## Domain Overview

### API

Provides common functionality to implement a GraphQL API.

### Application

Provides [Query/Command]Handler base classes to aid module teams in development of the CQRS pattern using MassTransit.
Also contains generic implementations for common response types and required infrastructure operations.

### Common

Provides common functionality that does not neatly fit into any more specific category. This could include various
algorithm implementations or custom data structures.

### Domain

Provides base classes that modules can use to implement a strongly typed domain layer.

### MongoDB

Provides a base integration with MongoDB that allows module teams to easily connect to and implement custom operations.

### OuranosML

Provides a base integration with OuranosML that module teams can use to perform various machine learning operations. As
of the writing of this, the OuranosML module is stored in a separate, private repository.

### RabbitMQ

Provides a base integration with RabbitMQ using MassTransit that allows module teams to produce and consume messages
from a RabbitMQ service.

### WebSockets

Provides a base integration for a WebSocket background job that allows module teams to easily connect to and communicate
with external WebSocket servers.

## Dependencies

- GraphQL
- MongoDB
- RabbitMQ
