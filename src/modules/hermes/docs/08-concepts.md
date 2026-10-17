# 8. Concepts: Hermes

## 8.1 Public Mode

Hermes reads the Flagsmith flag `hermes.public_mode` through `IFlagsmithClient`
([ADR 0012](../../../../docs/adr/0012-feature-flags-via-flagsmith.md)). While it is enabled,
the read handlers (conversations, folders, personas, traits, models) return only entities
whose `IsPublic` is set and treat the others as not found. Writes, completion and compaction
are not gated, so public mode limits what is listed, not what can be done.
