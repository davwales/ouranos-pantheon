# Features

Cross-cutting features that span multiple domains.

Each feature directory should follow the same pattern:

```
src/features/<feature-name>/
  ├── components/    # Feature-specific components
  ├── hooks/         # Feature-specific hooks
  ├── types.ts       # Feature-specific types
  └── utils.ts       # Feature-specific utilities
```

## Examples

- `auth/` - Authentication guards, login/logout flows
- `notifications/` - Toast/alert system
- `settings/` - User preferences
- `analytics/` - Page view tracking

## Guidelines

- Features should be self-contained and importable from any domain module.
- Features should **not** import from domain-specific directories (`hermes/`, `plutus/`).
- If a feature becomes domain-specific, move it to that domain's directory instead.
