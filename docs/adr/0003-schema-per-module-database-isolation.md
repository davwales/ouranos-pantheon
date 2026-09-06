# ADR 0003: Schema-per-module database isolation on PostgreSQL

**Status:** accepted

## Context

Modules own their data but share one PostgreSQL instance (see
[0001](0001-modular-monolith-over-microservices.md)). Without a rule, schemas drift into a
shared tangle: cross-module joins, ambiguous table ownership, migration collisions.

## Decision Drivers

- Hard data-ownership boundaries matching the module boundaries
- One database engine and one server to operate
- EF Core conventions must be uniform across modules
- Future extractability of a module (even if never exercised)

## Considered Options

- **One database, one schema per module**: `OuranosDbContext` subclasses declare their
  Postgres schema; snake_case naming via EFCore.NamingConventions
- One database, single shared schema: simplest, but no isolation
- Database per module: stronger isolation, more operational surface

## Decision

Each module gets its own **Postgres schema** owned by its own `DbContext`
(`plutus`, `hermes`, `hestia`, shared-owned tables for TickerQ/notifications). A shared
`PostgresModule` (`AddCorePostgresModule<TContext>` / `ApplyCorePostgresMigrations<TContext>`)
keeps DAL registration and migration application uniform. No module's context may map
entities of another module.

## Consequences

- Cross-module queries are impossible by construction: integration happens via API or
  messages, mirroring the assembly-reference rule
- Migration ownership is unambiguous per schema
- A module can later be extracted to its own database with data-copy tooling rather than
  code changes
- Slightly more connection/config surface than a single-schema database (negligible
  operationally)