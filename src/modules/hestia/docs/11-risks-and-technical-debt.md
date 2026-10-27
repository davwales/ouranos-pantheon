# 11. Risks and Technical Debt: Hestia

Module-scoped items. Platform-wide risks stay in
[platform Section 11](../../../../docs/architecture/11-risks-and-technical-debt.md); the
former platform IDs are noted so older references still resolve.

| # | Item | Where | Risk / impact | Proposed severity |
|---|------|-------|---------------|-------------------|
| HESTIA-D1 (was D10, Hestia part) | **Cross-slice type imports**: shopping-list manual items import another slice's schemas | `Features/ShoppingLists` | Slices are no longer independently changeable; shared types should move to the module's `Shared/` folder | Low |
| HESTIA-D2 (was D8, Hestia part) | *Resolved*: the empty `HestiaDbContext` was removed | - | - | - |
