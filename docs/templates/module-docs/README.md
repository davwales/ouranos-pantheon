# <Module> Module

<!--
Module documentation template. Copy this folder to src/modules/<module>/docs/ when creating
a module, replace every <placeholder>, and delete this comment.

Only this README is required. Create 05-building-block-view.md, 06-runtime-view.md,
08-concepts.md, 11-risks-and-technical-debt.md and 12-glossary.md the first time they have
content, and add them to Contents. Move a scenario or concept into runtime/<name>.md or
concepts/<name>.md once it outgrows its section.

Rules (see docs/architecture/README.md):
- Link up to platform docs freely; never expect platform docs to link into this folder
  except to this README.
- State each fact once, here, at the level that owns it.
- Refine platform decisions; never contradict them.
-->

<One paragraph: what the module does, in the module's own language.>

This is the module's arc42 entry point, refining the platform's building block view
([platform Section 5](../../../../docs/architecture/05-building-block-view.md)) one level
down. Platform-wide concepts are described in the
[platform documentation](../../../../docs/architecture/README.md).

## Contents

| Section | Focus |
|---------|-------|
| [Decisions](adr/README.md) | Module ADR log (`<MODULE>-ADR-*`) |

## Responsibilities

- <What the module is accountable for>

## Context

| Neighbour | Channel | Purpose | Code entry point |
|-----------|---------|---------|------------------|
| Interface | Inbound REST | <Module> pages under `src/apps/interface/src/app/<module>/` | Every `*Endpoint.cs` |

Code paths are relative to `Ouranos.Pantheon.Modules.<Module>/`. The module depends only on
the shared kernel.

## Provided Interfaces

- **REST**: routes under `/api/<module>/...`, one Bruno request per endpoint in
  `bruno/ouranos-pantheon/collections/API/<Module>/`.
- **SSE**: <streaming endpoints, or none>.

### Messages

| Message | Exchange | Queue | DLQ |
|---------|----------|-------|-----|

### Scheduled Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|

## Persistence

<Schema, persistence style, and anything unusual about it.>

## Configuration

`Ouranos:<Module>` (`<Module>Options`): <sections and what they control>.
