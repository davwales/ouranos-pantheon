# Ouranos Pantheon Architecture

This directory documents the architecture of Ouranos Pantheon following the
[arc42](https://arc42.org/overview) template. arc42 is a pragmatic, template-based
approach to architecture documentation: it defines twelve sections that answer the
questions *what* the system must do, *how* it is built, and *why* it is built that way.

**Primary audience:** engineers and reviewers evaluating the project as a portfolio of
modern .NET architecture patterns. Rationale gets as much space as facts; most sections
focus on *why*, not just *what*.

## Contents

| # | Section | Focus |
|---|---------|-------|
| 1 | [Introduction and Goals](01-introduction-and-goals.md) | Purpose, quality goals, stakeholders |
| 2 | [Architecture Constraints](02-architecture-constraints.md) | Technical & organizational constraints |
| 3 | [Context and Scope](03-context-and-scope.md) | External systems, interfaces, scope |
| 4 | [Solution Strategy](04-solution-strategy.md) | Fundamental approach and how it meets goals |
| 5 | [Building Block View](05-building-block-view.md) | Static decomposition: apps, modules, kernel |
| 6 | [Runtime View](06-runtime-view.md) | Key scenarios as sequences |
| 7 | [Deployment View](07-deployment-view.md) | Environment, artifacts, topology |
| 8 | [Crosscutting Concepts](08-crosscutting-concepts.md) | Domain primitives, messaging, persistence, conventions |
| 9 | [Architecture Decisions](09-architecture-decisions.md) | Index of decision records |
| 10 | [Quality Requirements](10-quality-requirements.md) | Quality tree and scenarios |
| 11 | [Risks and Technical Debt](11-risks-and-technical-debt.md) | Known risks and accepted debt |
| 12 | [Glossary](12-glossary.md) | Domain and technical vocabulary |

## Conventions

- **Diagrams** are [Mermaid](https://mermaid.js.org) blocks. They render natively on
  GitHub, diff well in PRs, and need no tooling. Node names in diagrams match Glossary
  terms.
- **Architecture Decision Records** live in [`docs/adr/`](../adr/README.md) using the
  [MADR](https://adr.github.io/madr/) format. Section 9 indexes them.
- **Terminology** follows the [Glossary](12-glossary.md). Module names (Plutus, Hermes,
  Hestia, Shared) are always capitalized as proper nouns.

## Maintaining this documentation

Architectural changes (new/renamed module, changed messaging topology, changed external
integrations, altered crosscutting conventions) must update the relevant section here
and, when a real trade-off is involved, add a new ADR under `docs/adr/`. Update diagrams
in the same change as the code they describe.
