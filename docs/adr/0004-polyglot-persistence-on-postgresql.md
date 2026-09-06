# ADR 0004: Polyglot persistence on one PostgreSQL engine (TimescaleDB + Marten)

**Status:** accepted

## Context

Modules have genuinely different persistence shapes: Plutus stores high-volume
time-series trades and signals with market-wide rollups; Hestia is inherently
event-sourced (full recipe version history with revert); Hermes is classic relational
data. A single access strategy (plain EF Core) would fight at least two of these domains.

## Decision Drivers

- Time-series performance and automatic rollups for market data
- Native event store with versioning/revert semantics for recipes
- Uniform relational modeling where nothing special is needed
- Exactly one database server to operate (homelab constraint)

## Considered Options

- **One PostgreSQL instance, style per module**: EF Core (+ TimescaleDB hypertables and
  continuous aggregates) for time-series; Marten event streams for Hestia; plain EF Core
  elsewhere
- Plain EF Core everywhere: one style, but hand-rolled rollups and hand-rolled event
  history
- Dedicated engines (ClickHouse, EventStoreDB): best-of-breed per style, more services
  to run

## Decision

Keep **one engine, two styles**: TimescaleDB extension for Plutus's trade/signal
hypertables and continuous aggregates (used by market views), and Marten for Hestia's
event-sourced recipe aggregates (inline snapshots, Guid stream keys). Hermes and shared
infrastructure use plain EF Core relational mapping. All on the same PostgreSQL server,
each in its module schema ([0003](0003-schema-per-module-database-isolation.md)).

## Consequences

- Rollups and time-partitioning are declarative (TimescaleDB) instead of hand-written
- Recipe history/revert is a library capability instead of custom audit tables
- Two ORMs/persistence mental models to know (EF Core + Marten); the cost is concentrated
  in two modules
- MongoDB.Bson is used only as a BSON parser for Universalis; no MongoDB server exists
  in this system