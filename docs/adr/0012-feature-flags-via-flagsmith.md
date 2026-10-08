# ADR 0012: Feature flags via Flagsmith; public mode as data-exposure control

**Status:** accepted

## Context

Recorded 2026-10-07; the decision predates this record.

Hermes holds personal chat history, personas, and model configurations. The owner
sometimes wants to show the chat UI to others, for example as a portfolio demo, without
exposing everything stored in it. [ADR 0008](0008-no-authentication-single-user.md)
rules out authentication, so there is no user identity to filter on. What the system
needed was a switch that can be flipped at runtime, without a redeploy, to limit reads
to entities the owner has marked as public.

## Decision Drivers

- Toggle behaviour at runtime without rebuilding or restarting the gateway
- No authentication or identity model (ADR 0008 stays in force)
- Small surface: one module needs it today

## Considered Options

- **Flagsmith**: hosted flag service with a UI, evaluated by an `IFlagsmithClient`
  registered once in the Shared module
- `IConfiguration` / appsettings toggles: no new dependency, but changing a value
  means a redeploy or a config reload on the host
- Microsoft.FeatureManagement: richer in-process filters, still backed by
  configuration, so it has the same redeploy problem
- No flags: maintain a separate demo deployment or dataset

## Decision

Use **Flagsmith** for feature flags. The Shared module registers the client from
`Ouranos:Flagsmith`. Hermes defines one flag, `hermes.public_mode`; while it is enabled
the Hermes read handlers (conversations, folders, personas, traits, models) return only
entities whose `IsPublic` is set and treat non-public ones as not found. Public mode
does not authenticate anyone and does not supersede ADR 0008. It narrows what an
unauthenticated viewer can see.

## Consequences

- Demo exposure can be switched on and off in seconds from the Flagsmith UI
- Entities carry an explicit `IsPublic` decision, which documents intent per record
- Hermes reads depend on one more runtime service; with no `Ouranos:Flagsmith:ApiUrl`
  configured, the client cannot be constructed and those reads fail
- The filter is only as complete as the handlers that check the flag. Write endpoints,
  completion, and compaction are not gated, so public mode limits what is listed, not
  what can be done
- It is not a security boundary; the API is still open on the network
  ([§8.13](../architecture/08-crosscutting-concepts.md#813-security))
