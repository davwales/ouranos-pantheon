# <Module> Architecture Decision Records

<!-- Links in this template are written for its destination, src/modules/<module>/docs/adr/. -->

Decisions whose consequences stay inside the <Module> module. They use the same
[MADR](https://adr.github.io/madr/) short form as the
[platform ADRs](../../../../../docs/adr/README.md) and are cited as `<MODULE>-ADR-NNNN`.

A decision belongs here only if it meets the platform bar (a major decision with lasting
consequences and real alternatives) **and** affects no other module. A module ADR refines
the platform decisions and never contradicts them; if it would, the decision is a platform
decision. Restructures inside the module that follow established patterns need no ADR.

## Index

No module-level decisions have been recorded yet. Add a `| # | Decision | Status |` table
with the first one.

## Process

1. Copy [the template](../../../../../docs/templates/module-docs/adr/0000-template.md) to
   `NNNN-<decision-shaped-title>.md` with the next number in this log.
2. Add a row to the index above. Module ADRs are not listed in the platform index.
3. If a decision changes, mark the old record superseded and write a new one.
