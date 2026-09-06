# 1. Introduction and Goals

## 1.1 Purpose

Ouranos Pantheon is an **extensible modular monolith** for centralized personal services.
It aggregates and analyzes market data across game economies and financial markets
(FFXIV, OSRS, US equities; **Plutus**), provides configurable AI chat assistants over
locally hosted LLMs (**Hermes**), and manages recipes with version history and automated
web import (**Hestia**).

Beyond its day-to-day utility, the project exists to demonstrate modern .NET
architecture patterns in a real application: Vertical Slice
Architecture, Domain-Driven Design, message-driven pipelines, event sourcing, and a
module contract that keeps adding a new domain explicit and cheap.

This documentation follows the arc42 template; see the [index](README.md) for how it is
organized.

## 1.2 Quality Goals

These quality goals are ranked; they drive the strategy in [Section 4](04-solution-strategy.md):

| Priority | Quality goal | Motivation |
|----------|--------------|------------|
| Q1 | **Extensibility**: a new domain is a new module, not a change to existing modules | The platform is designed to grow, module by module |
| Q2 | **Design clarity**: patterns are idiomatic, explainable, and traceable to rationale | Core project purpose as a portfolio showcase |
| Q3 | **Operational simplicity**: one deployable, one database engine, one broker, self-hosted | Single-operator constraint ([Section 2](02-architecture-constraints.md)) |

## 1.3 Stakeholders

| Stakeholder | Role | Expectation |
|-------------|------|-------------|
| Owner (developer & user) | Builds, operates, and uses the system daily | Reliable personal service; enjoyable, instructive codebase |
| Portfolio readers / peer engineers | Evaluate the architecture and patterns | Honest rationale, including trade-offs and known debt (Section 11) |

## 1.4 Document Scope

Sections 3–8 are derived from the repository (code, configuration, CI) and kept factual.
Sections 1, 4, 9, 10, and 11 encode intent and judgment; treat them as living documents.
