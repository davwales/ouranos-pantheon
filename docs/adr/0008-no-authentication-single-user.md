# ADR 0008: No authentication for the single-user, trusted-network deployment

**Status:** accepted

## Context

The gateway is deployed on a private homelab network for exactly one known user. Adding
authentication means an identity provider or user store, token lifecycle, and frontend
auth flows, all for an audience of one.

## Decision Drivers

- Single user, single trusted network
- Minimize operational and frontend complexity
- The API surface is the same one the owner's browser uses; there is no public exposure

## Considered Options

- **No authentication**: network location is the perimeter; CORS allow-list for browser
  origins
- Lightweight API key: cheap to add, weak guarantee, secret management for one user
- Full auth (OpenID Connect / provider): future-proof for any public exposure, heavy for
  today

## Decision

Ship **without authentication**. Protections: private network placement, a CORS
allow-list policy (`AllowLocalAndServer`), and an anti-SSRF guard on the outbound recipe
scraper. Any future public exposure requires revisiting this decision first.

## Consequences

- Simplest possible frontend and CLI integration
- Every host on the home network has full API access, including destructive operations
  (strategy deletion, recipe import). Recorded as accepted risk
  [D2](../architecture/11-risks-and-technical-debt.md)
