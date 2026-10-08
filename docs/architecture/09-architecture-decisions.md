# 9. Architecture Decisions

Significant architecture decisions are recorded as MADR files in
[`docs/adr/`](../adr/README.md). **The index lives there**; this section only highlights
the decisions with the widest reach. How each decision serves the quality goals is
tabulated in [Section 4](04-solution-strategy.md).

## The Decisions That Shape Everything

Three decisions explain most of the architecture's texture:

1. **[ADR 0001](../adr/0001-modular-monolith-over-microservices.md), modular monolith**:
   the module is the unit of extension; the gateway composes; modules share only the
   kernel.
2. **[ADR 0002](../adr/0002-vertical-slice-architecture-with-wolverine.md), vertical slices + Wolverine**:
   the unit of change is the feature folder; endpoints reach handlers only through the
   message bus, and slices never call each other's handlers.
3. **[ADR 0004](../adr/0004-polyglot-persistence-on-postgresql.md), polyglot persistence on one engine**:
   each domain gets the persistence style it needs (hypertables, event streams, plain
   relational) without multiplying infrastructure.

New decisions follow the process in [`docs/adr/README.md`](../adr/README.md).
