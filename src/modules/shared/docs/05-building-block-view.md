# 5. Building Block View: Shared

Level-2 whitebox of the two Shared assemblies described in the [README](README.md).

## 5.1 `Ouranos.Pantheon.Modules.Shared.Contract` (kernel)

Referenced by every module. It holds no domain logic.

| Building block | Responsibility |
|----------------|----------------|
| `IPantheonModule.cs` | The module contract: `Build`, `Configure`, optional `MapEndpoints` and `ConfigureWolverine` |
| `Domain/` | `Id<T>`, `BaseEntity`, `BaseEventSourcedEntity`, `IDomainEvent`, `OperationResult<T>` |
| `Application/` | Handler markers (`IPantheonHandler`, `IPantheonStreamHandler`), the common query contract (paging, sorting, filtering), and the AI assistant framework (`AssistantToolLoop<TContext>`, `PantheonAssistant<TContext>`, `AssistantTool<TContext, TArguments>`) |
| `API/` | `SseWriter` and `MapAssistant` (`AssistantEndpointExtensions`) |
| `WebSockets/` | `WebSocketWorker` infrastructure: listener registry, reconnecting client, serializers, health state, telemetry |
| `Infra/OuranosMachineLearning/` | The OuranosMl client and the global model defaults (`ModelDefaults`) |
| `Infra/Postgres/` | `PostgresModule`, `OuranosDbContext`, and the Marten registration helper (`AddCoreMartenModule`) |
| `Algorithms/` | Generic algorithms, e.g. the genetic-algorithm engine |

## 5.2 `Ouranos.Pantheon.Modules.Shared` (module)

The first entry in the gateway's module array, referenced only by the gateway.

| Building block | Responsibility |
|----------------|----------------|
| `API/Extensions/CoreExtensions.cs` | Core composition (`AddOuranosCore`, `UseOuranosCore`) and the Wolverine error policy |
| `API/ExceptionHandlers/` | Maps `InvalidFilterException` to 400 ProblemDetails |
| `Infra/RabbitMq/` | RabbitMQ option wiring |
| `Features/Health/` | The health endpoint and its checks (Postgres, RabbitMQ, OuranosMl, WebSocket, TickerQ) |
| `Infra/Observability/` | OpenTelemetry tracing and metrics registration |
| `Infra/Flagsmith/` | Flagsmith client registration |
| `Infra/Postgres/Notifications/` | Notification entities and the `NotificationSender` job |
| `Migrations/` | `SharedDbContext` and TickerQ store migrations |

## 5.3 Interface Components

The assistant UI that every module page uses lives in the Interface app at
`src/apps/interface/src/components/shared/ai-assistant/`; it is documented with the
assistant framework in [Concepts](08-concepts.md#82-frontend).
