# 11. Risks and Technical Debt: Shared

Shared-scoped items. Risks that affect the whole host (single-instance assumptions, runtime
code generation, the unauthenticated TickerQ dashboard) stay in
[platform Section 11](../../../../docs/architecture/11-risks-and-technical-debt.md).

| # | Item | Where | Risk / impact | Proposed severity |
|---|------|-------|---------------|-------------------|
| SHARED-D1 (was D8, Shared part) | **Unused scaffolding**: the notification outbox and its every-minute job have no sender implementation (tracked in #7) | `Infra/Postgres/Notifications/` | Readers assume capabilities that do not exist; dead code carries test and maintenance cost | Low |
