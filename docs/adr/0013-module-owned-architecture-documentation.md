# ADR 0013: Module-owned architecture documentation

**Status:** accepted

## Context

The arc42 sections in `docs/architecture/` and the ADR log in `docs/adr/` were written as
one platform-wide set ([ADR 0000](0000-use-madr-format.md)). Module detail accumulated in
them. In October 2026 the module-specific share was about 80% of the runtime view, about
45% of the building block view and about 55% of the glossary. Of the 28 commits that had
touched `docs/`, about 16 were module feature work: assistants, strategies, forecasts.

The code keeps modules isolated ([ADR 0001](0001-modular-monolith-over-microservices.md),
[ADR 0003](0003-schema-per-module-database-isolation.md)), but the documentation did not.
Nearly every module change edited the same few shared files (`06-runtime-view.md`,
`08-crosscutting-concepts.md`, `05-building-block-view.md`). With one team per module, those
files would be a constant source of merge conflicts and cross-team review. A module also
could not be extracted without untangling its rationale from shared files.

## Decision Drivers

- A module's change, including its documentation, stays inside the module's folder
- Keep arc42 and MADR as the formats
- A reader can still get the whole picture from one entry point
- Documentation of a module can move with the module if it is ever extracted
- Low overhead: no tooling, no generated docs

## Considered Options

- **Federated arc42**: the platform describes each module as a blackbox, and each module
  keeps a small arc42 subset (README blackbox, then building blocks, runtime, concepts, risks
  and glossary as needed) plus its own MADR log in `src/modules/<module>/docs/`
- One central set (status quo): simple to find, but every module edits the same files
- A module README plus one document per feature: easy to start, but in Vertical Slice
  Architecture slices are already self-describing, and the scenarios worth documenting
  span several slices. The file count also grows with the number of features, and the result
  is no longer arc42
- Module folders under `docs/modules/<module>/`: avoids file conflicts, but the docs stay on
  shared ground away from the code, so path-based ownership (`CODEOWNERS`) and module
  extraction have to treat code and docs separately

## Decision

Adopt **federated arc42**. arc42's building block view is hierarchical by design: level 1
(the platform) shows modules as blackboxes, and level 2 is each module's whitebox. Each
module's `docs/` folder holds that level:

- `README.md` (required) is the module's blackbox and the only file the platform links to.
- `05-building-block-view.md`, `06-runtime-view.md`, `08-concepts.md`,
  `11-risks-and-technical-debt.md` and `12-glossary.md` are created when they have content.
  They keep the platform's section numbers. A scenario or concept that outgrows its section
  moves to `runtime/<name>.md` or `concepts/<name>.md`.
- `adr/` is the module's MADR log, cited as `<MODULE>-ADR-NNNN`. It uses the platform's bar
  for what deserves an ADR.

A change is documented at platform level only if it alters the module contract or shared
kernel conventions, adds host infrastructure, crosses module boundaries, adds or renames a
module, or changes deployment, CI, quality goals or constraints. Everything else is
documented by the module. Module docs may link anywhere in the platform docs; platform docs
link to a module only through its README. A module ADR refines the platform decisions and
never contradicts them.

Existing ADRs stay in the platform log, since all of them constrain more than one module.
Module content moved out of the platform sections, and debt items that moved keep stub rows
so their IDs are never reused.

## Consequences

- Module work no longer touches `docs/`; platform docs change only with platform work
- Code and docs share a path, so per-module ownership can later be enforced with `CODEOWNERS`
- A module extracted into its own service takes its docs and decision log with it
- Each module has its own ubiquitous language, so homonyms such as *Recipe* (Plutus
  crafting, Hestia cooking) are fine without a shared disambiguation table
- The full picture is spread across more files. The platform index and Section 5 point to
  every module README as a stable entry point.
- Cross-module features are documented twice: each side in its module, and the contract
  between them in the platform docs. That cost is intended, because coupling between modules
  should be visible.
- Contributors must decide which level a change belongs to. `AGENTS.md` and
  `docs/architecture/README.md` give the test.
