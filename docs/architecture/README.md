# Ouranos Pantheon Architecture

This directory documents the architecture of Ouranos Pantheon following the
[arc42](https://arc42.org/overview) template. arc42 is a pragmatic, template-based
approach to architecture documentation: it defines twelve sections that answer the
questions *what* the system must do, *how* it is built, and *why* it is built that way.

The documentation is federated: this directory describes the platform, and each module
documents itself one level down in `src/modules/<module>/docs/` (see
[Module documentation](#module-documentation)).

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
| 5 | [Building Block View](05-building-block-view.md) | Static decomposition: apps, modules as blackboxes, kernel |
| 6 | [Runtime View](06-runtime-view.md) | Platform scenarios as sequences |
| 7 | [Deployment View](07-deployment-view.md) | Environment, artifacts, topology |
| 8 | [Crosscutting Concepts](08-crosscutting-concepts.md) | Domain primitives, messaging, persistence, conventions |
| 9 | [Architecture Decisions](09-architecture-decisions.md) | Index of decision records |
| 10 | [Quality Requirements](10-quality-requirements.md) | Quality tree and scenarios |
| 11 | [Risks and Technical Debt](11-risks-and-technical-debt.md) | Known risks and accepted debt |
| 12 | [Glossary](12-glossary.md) | Platform and technical vocabulary |

## Module documentation

Each module keeps a small arc42 subset next to its code. Only the README is required; the
other files exist once they have content, and keep the platform's section numbers so a
reader knows where to look.

| Module | Entry point |
|--------|-------------|
| Plutus | [`src/modules/plutus/docs/`](../../src/modules/plutus/docs/README.md) |
| Hermes | [`src/modules/hermes/docs/`](../../src/modules/hermes/docs/README.md) |
| Hestia | [`src/modules/hestia/docs/`](../../src/modules/hestia/docs/README.md) |
| Shared | [`src/modules/shared/docs/`](../../src/modules/shared/docs/README.md) |

```
src/modules/<module>/docs/
  README.md                       required: blackbox (purpose, context, interfaces,
                                  messages, jobs, persistence, configuration) + index
  05-building-block-view.md       level-2 whitebox: the module's slices
  06-runtime-view.md              module scenarios (large ones in runtime/<name>.md)
  08-concepts.md                  module-specific concepts (assistants, tools, …)
  11-risks-and-technical-debt.md  module debt, IDs prefixed <MODULE>-D
  12-glossary.md                  the module's ubiquitous language
  adr/README.md                   module ADR log, cited as <MODULE>-ADR-NNNN
```

New modules start from [`docs/templates/module-docs/`](../templates/module-docs/README.md).

## Conventions

- **Diagrams** are [Mermaid](https://mermaid.js.org) blocks. They render natively on
  GitHub, diff well in PRs, and need no tooling. Node names in diagrams match the glossary
  at the same level (platform or module).
- **Architecture Decision Records** live in [`docs/adr/`](../adr/README.md) using the
  [MADR](https://adr.github.io/madr/) format. Section 9 indexes them. Module-scoped
  decisions live in each module's `docs/adr/`.
- **Terminology** follows the [Glossary](12-glossary.md). Module names (Plutus, Hermes,
  Hestia, Shared) are always capitalized as proper nouns.

## Maintaining this documentation

Document a change at the level that owns it
([ADR 0013](../adr/0013-module-owned-architecture-documentation.md)), in the same change as
the code it describes, diagrams included.

**Platform docs** (`docs/`) change when a change:

- alters the module contract (`IPantheonModule`, the shared kernel's public surface, a
  convention in Section 8)
- adds infrastructure the host must provide (database engine, broker, observability backend,
  platform service)
- adds a message, call or integration that crosses module boundaries
- adds, renames or removes a module
- changes deployment, CI, quality goals or constraints

**Module docs** (`src/modules/<module>/docs/`) take everything else: new or changed slices,
module-internal messages and queues, scheduled jobs, configuration keys, an external API
only that module consumes, assistants and their tools, module debt and vocabulary.

Rules that keep modules independent:

- **Links follow the code's dependency direction.** Module docs may link to any platform
  doc. Platform docs link to a module only through its `README.md`, never into its other
  files or anchors, so a module can restructure its docs freely.
- **State each fact once, at the lowest level that owns it.** The platform names a module
  and links to it; the module holds the facts.
- **Module docs refine, never override.** A module ADR cannot contradict a platform ADR;
  if it needs to, the decision is a platform decision.
