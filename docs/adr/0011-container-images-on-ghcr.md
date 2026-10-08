# ADR 0011: Container images on GHCR, deployment owned by the infrastructure repo

**Status:** accepted

## Context

Recorded 2026-10-07; the decision predates this record.

The system ships as two processes, the gateway (which hosts every module, worker, and
scheduled job) and the Next.js interface, and runs on a single homelab host next to
PostgreSQL, RabbitMQ, Flagsmith, and the observability stack. Those infrastructure
services are shared with other homelab workloads and are already provisioned from a
separate `ouranos-infrastructure` repository. Something has to turn a commit on `main`
into a runnable artifact, and something has to decide how and where it runs.

## Decision Drivers

- One person operates everything; releasing should need no manual build step
- No registry credentials or secrets to manage beyond what CI already has
- Infrastructure is shared beyond this application and already versioned elsewhere
- Keep this repository about the application, not about one host's topology

## Considered Options

- **Publish images to GHCR from CI; the infrastructure repo owns runtime topology**
- Docker Compose files in this repository: deployment versioned with the code, but
  duplicates (or forks) the shared infrastructure definitions
- Kubernetes manifests: rolling updates and rollbacks, far more machinery than a
  single host and a single user need
- Build on the host from source: no registry at all, but builds compete with the
  running system and a toolchain must live on the server

## Decision

CI builds one image per deployable (gateway, interface) from the Dockerfiles in this
repository and pushes them to GitHub Container Registry on every push to `main`,
authenticating with the workflow's own `GITHUB_TOKEN`. This repository stops at the
image. Compose definitions, environment variables, ports, and promotion onto the host
live in `ouranos-infrastructure`.

## Consequences

- Releasing is merging to `main`; no registry secrets exist outside GitHub
- The application repository stays free of host-specific configuration
- Images are tagged `:latest` only, so there is no rollback target and no link from a
  running container back to a commit ([D13](../architecture/11-risks-and-technical-debt.md))
- `NEXT_PUBLIC_*` values are inlined when the interface image is built, so the API
  base URL is fixed at build time rather than set at deploy time
- Runtime configuration is split across two repositories; a new setting needs a change
  in both, and nothing checks they agree
- Promotion onto the host is manual
  ([D6](../architecture/11-risks-and-technical-debt.md))
