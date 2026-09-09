# ADR 0009: OpenTelemetry traces and metrics exported to Grafana via OTLP

**Status:** accepted

## Context

Production observability was logs only: Serilog ships to Grafana Loki and health checks
cover dependency state. Diagnosing the gateway's flows meant correlating log lines by
hand, because request paths cross several async boundaries: inbound HTTP, Wolverine
handlers, RabbitMQ consumers, EF Core/Npgsql queries, Marten sessions, and outbound HTTP
(data loaders, OuranosMl, recipe scraper). Background processes left no signal at all:
TickerQ jobs execute outside any trace, `ClientWebSocket` data loaders bypass the
HttpClient instrumentation entirely, and no library meter (Npgsql, EF Core, Wolverine,
Marten, ASP.NET Core, runtime) was ever subscribed, so the metrics signal was empty.

The homelab already runs a Grafana stack with Loki; the infrastructure repository stands
up Grafana Alloy, which receives OTLP and forwards it into the Grafana stack.

## Decision Drivers

- End-to-end visibility of HTTP → messaging → database flows without hand-rolled timing
- Instrument libraries already in use (ASP.NET Core, HttpClient, Npgsql, Wolverine,
  Marten, TickerQ) instead of writing span code; manual instrumentation only where no
  library option exists
- Keep the log pipeline (Serilog → Loki) as the single log path
- Configuration-driven; silent no-op in environments without an OTLP endpoint
- Background trace volume must not drown the real request traces

## Considered Options

- **OpenTelemetry .NET SDK + signal-specific OTLP exporters** targeting the self-hosted
  Grafana stack (Alloy receiver)
- Grafana Cloud OTLP endpoint: managed, but another external dependency and
  authentication surface for a homelab
- Zero-code auto-instrumentation (OTel .NET automatic instrumentation): deployment-level
  env vars, less control over filtering and sampling
- No tracing, log correlation only: insufficient for the async message pipelines

## Decision

Adopt the **OpenTelemetry .NET SDK**, registered centrally in `AddOuranosCore` via
`AddCoreObservabilityModule` (Shared module, `Infra/Observability/`), exporting
**traces and metrics** over OTLP to the Grafana stack. The detailed state lives in
[arc42 §8.13](../architecture/08-crosscutting-concepts.md#813-observability-opentelemetry);
the decisions recorded here are:

1. **Library instrumentation first.** Coverage follows a preference order: library
   packages (no code) → refactoring to enable auto-instrumentation → manual code.
   Manual code is confined to the shared WebSocket abstractions
   (`WebSocketTelemetry`), because `ClientWebSocket` has no instrumentation option
   anywhere in the OTel .NET ecosystem. The alternative refactor that would have
   produced free auto-spans (routing raw socket frames through Wolverine local
   messaging) is rejected: it would change the runtime characteristics of the
   highest-frequency path in the system, and ingestion behaviour must not become an
   implementation detail of observability. Each `websocket message` span is a trace
   root by design: the receive loop runs inside the ambient context of the
   session-long `websocket connect` span, and without clearing it every message of a
   connection would parent to that span, producing one unbounded megatrace per
   connection. With message spans as roots, `SamplingRatio` also throttles feed
   traces.
2. **Logs stay on Serilog → Loki.** The OTel log exporter stays disabled (signal-specific
   `AddOtlpExporter()` on the tracing and metrics pipelines, not `UseOtlpExporter()`).
   Consolidating logs into OTel was considered and rejected; Serilog 4.x populates
   `TraceId`/`SpanId` from the current activity, and the Grafana Loki sink (v9) writes
   them into the JSON log body (`traceIdMode`/`spanIdMode: Body`), giving log↔trace
   correlation through a Loki datasource derived field (configured in Grafana, not in
   this repository). This required upgrading the sink from v8, which never serialized
   the trace context.
3. **Background root-span exclusion.** The tracing sampler is a custom
   `RootSpanExclusionSampler` wrapping `ParentBasedSampler(TraceIdRatioBasedSampler)`:
   root activities (no parent) whose name matches
   `Ouranos:Observability:ExcludedRootSpanNames` (trailing `*` matches by prefix) are
   dropped, while matching spans under a recorded parent stay as children. This is a
   deliberate visibility cut. Measured against a live Tempo, unfiltered background
   infrastructure (Wolverine durability queries, TickerQ's operational store, bare
   Npgsql connections) produced ~160 orphaned `postgresql`/`CONNECT` traces/min against
   a handful of real request traces. Background DB work therefore surfaces through
   metrics and logs only; the exclusion list is tuned per environment.
4. **Exporter gating.** When `Ouranos:Observability:OtlpEndpoint` is empty and
   `OTEL_EXPORTER_OTLP_ENDPOINT` is unset, no exporter is registered and signals are
   dropped (local dev stays quiet).

## Consequences

- One trace covers a request from the REST endpoint through Wolverine consumers to
  Postgres calls and outbound HTTP; TickerQ jobs, websocket messages, and message
  handlers reach the database inside their own traces with zero hand-written span code
  on feed and job code. Websocket publishes from listeners inherit the message span,
  so socket → RabbitMQ → consumer → Postgres is one trace per message; the
  `websocket connect`/`websocket disconnect` spans are their own traces.
- Dropped root spans are invisible as traces by design. Someone debugging a background
  job will not find its bare `postgresql` roots in Tempo; that is the accepted cut, not
  a tracing defect. If feed traffic pressures volume, lower
  `Ouranos:Observability:SamplingRatio` or extend the exclusion list.
- The Loki sink v9 upgrade changes the production log pipeline: it batches natively
  (the `Async` wrapper was removed) and Grafana's level vocabulary uses `fatal` where
  v8 used `critical`. Loki ingestion needs a post-deploy check.
- The Wolverine meter subscription must use the `Wolverine*` wildcard (the meter name
  embeds the application name); a bare `AddMeter("Wolverine")` captures nothing.
- An unreachable OTLP endpoint produces exporter error logs in the background, not
  request failures (batch processor retries).
- Frontend (browser) telemetry remains out of scope; the RabbitMQ client itself is not
  instrumented (Wolverine's transport spans represent the messaging path).
