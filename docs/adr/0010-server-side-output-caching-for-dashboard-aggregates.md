# ADR 0010: Server-side output caching for hot dashboard aggregates

**Status:** accepted

## Context

The market overview and volume-heatmap endpoints are the two hottest read paths in the
dashboard: the frontend polls both while a market page is open. Under load (k6 stress
testing) their request latencies ramp to multiple seconds because every request
recomputes the aggregation in Postgres - the heatmap rescanned up to ~1.5M raw trade
rows per request, and the overview view scanned its window twice (once to derive the
bucket interval, once to aggregate). The aggregates these endpoints produce are
dashboard visualizations whose values change slowly; recomputing them on every request
buys nothing.

## Decision Drivers

- Absorb poll traffic so stress load scales with unique keys, not request count
- No new infrastructure components (single gateway host, single Postgres)
- Bounded staleness that is invisible on a dashboard (sub-minute)
- Keep invalidation simple; no event-driven invalidation plumbing

## Considered Options

- **ASP.NET Core OutputCache with short TTL, configured inline per endpoint**: in-process,
  per-endpoint opt-in at the mapping site, no dependencies
- Named output-cache policies registered centrally or per module: adds indirection
  between an endpoint and its cache behavior
- HybridCache (L1 + L2 Redis): adds a Redis dependency for a single-host deployment
- Event-driven invalidation on trade ingestion: exact freshness, but couples the
  ingestion pipeline to read endpoints
- No caching, only DB-side optimization: fixes the scan cost but every request still
  pays it

## Decision

Use **ASP.NET Core OutputCache**, configured inline in each endpoint file at the mapping
site via `.CacheOutput(policy => ...)` (30-second expiry, vary-by query keys referenced
with `nameof(InputSchema.Property)` so they stay strongly typed against the schemas the
endpoints bind from; route parameters like market id are part of the key by default).
The Shared module wires only the mechanism: `AddOutputCache()` / `UseOutputCache()` in
`CoreExtensions`; no named policies or module-level registration.

## Consequences

- Dashboard data can be up to 30 seconds stale; acceptable for aggregate visualizations
- Under load, identical requests collapse into one computation per TTL window
- A multi-replica gateway would cache per replica (per-instance memory only); move to
  HybridCache with a shared L2 if replicas are ever introduced
- Only successful responses are cached; errors are never served from cache
- Each slice owns its caching inline; adding a cached endpoint requires no changes
  outside the endpoint file
