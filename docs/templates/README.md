# Templates

Platform-wide templates used by every module.

| Template | Use for |
|----------|---------|
| [issues/feature.md](issues/feature.md) | New capability or behaviour change |
| [issues/bug.md](issues/bug.md) | Something broken |
| [issues/tech-debt.md](issues/tech-debt.md) | Cleanup with no user-facing change |
| [pull-request.md](pull-request.md) | Every pull request |
| [module-docs/](module-docs/README.md) | Documentation for a new module |

## Conventions

- **Issue titles:** `<Module> | <Title>`, where Module is `Core`, `Hermes`, `Hestia` or `Plutus`.
- **PR titles:** Conventional Commits, `<type>(<scope>): <summary>`.
- **Keep it short.** A few bullets per section; skip anything the diff or code already says.
