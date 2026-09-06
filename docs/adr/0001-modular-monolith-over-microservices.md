# ADR 0001: Modular monolith over microservices

**Status:** accepted

## Context

The platform hosts multiple unrelated domains (market data, AI chat, recipes) that will
grow independently over years. Each domain could plausibly be its own service. The system
is built and operated by a single developer on a homelab network.

## Decision Drivers

- Independent domain growth without touching existing code (extensibility)
- Operational simplicity: one person operates everything
- Meaningful demonstration of module boundary patterns (portfolio value)
- All domains share the same infrastructure (PostgreSQL, RabbitMQ)
- The scaling benefits of a microservice architecture are moot when the application will only be run on a single machine (the homelab)

## Considered Options

- **Modular monolith**: modules composed in one host behind a module contract
- Microservices: one deployable per domain, network calls between them
- Single-project layered monolith: no module boundaries at all

## Decision

Build a **modular monolith**: each domain is a self-contained module implementing
`IPantheonModule`, referencing only the shared kernel. A thin gateway composes the module
array. No module references another module's assembly; no network calls between domains.

## Consequences

- Adding a domain is additive (new module + one registration line)
- No distributed-systems concerns (no service discovery, no network serialization
  between domains, single transactional scope)
- All modules deploy and scale together, which is acceptable for a single user
- Boundary discipline depends on the reference rule + review, since the compiler cannot
  enforce intra-module layering in single-assembly modules
