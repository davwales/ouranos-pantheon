# ADR 0000: Record architecture decisions with MADR

**Status:** accepted

## Context

Architectural decisions in this repository were implicit until this file: they are
visible only in code shape and git history. For a portfolio project, the *why* is as
valuable as the *what*, and future changes need a record of prior rationale to avoid
relitigating settled questions.

## Decision Drivers

- Decisions and their rationale must be reviewable in pull requests
- Minimal process overhead for a single developer
- Standard format recognizable to other engineers

## Considered Options

- **MADR (Markdown ADRs)**: plain Markdown, lightweight, widely used
- Y-statements only: very compact but loses alternatives detail
- Decision log in a single file: poor diff granularity, no per-decision linking

## Decision

Adopt MADR in short form. Records live in `docs/adr/`, numbered sequentially, indexed by
`README.md` and linked from arc42 [Section 9](../architecture/09-architecture-decisions.md).

## Consequences

- Every significant trade-off gets a small Markdown file and a PR
- The index must be maintained (one table row per ADR)
- Superseded decisions remain in place, linked to their replacements