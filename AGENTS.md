# AGENTS.md

Compact guide for agentic coding sessions: high-signal, repo-specific facts only. The human-facing
overview (modules, setup, configuration) lives in [`README.md`](README.md). If a gitignored
`AGENTS.local.md` exists, read it too - it holds environment-specific data access and tooling.

## Hard Rules

1. **⛔ Run `automation/format.sh` after changing any `.cs`, `.ts`, or `.tsx` file.** CI rejects
   unformatted code (see [Formatting](#formatting)).
2. **⛔ Never stage or commit on your own initiative.** Leave changes unstaged until the user asks
   you to stage, commit or push them. Offering is fine, especially for small, targeted changesets:
   ask once the work is done and verified. Approval covers only that changeset.
3. **⛔ No null-forgiving operator (`!`) in production code** (see [.NET](#net)).
4. **⛔ Endpoint changes update Bruno** in the same change (see [Bruno](#bruno-api-collection)).
5. **⛔ Architectural changes update arc42 at the level that owns them** in the same change: module
   changes in `src/modules/<module>/docs/`, platform changes in `docs/` (see
   [Docs](#architecture-documentation-arc42--adrs)).
6. **Comment only the non-obvious *why*** (workarounds, business rules, constraints), in C# and
   TypeScript. Code must otherwise be self-documenting. The test `// Arrange` / `// Act` /
   `// Assert` markers are the only structural comments.

## Project Overview

Modular monolith: .NET 10 backend (ASP.NET Core Minimal APIs + Wolverine), Next.js 16 / React 19 /
TypeScript frontend. Vertical Slice Architecture with DDD. Three domain modules: **Plutus**
(market data, signals, strategies/backtests), **Hermes** (AI chat), **Hestia** (recipes,
event-sourced via Marten); **Shared** (`Modules.Shared` + `Modules.Shared.Contract`) provides the
kernel. The **Gateway** (`src/apps/gateway/Ouranos.Pantheon.Apps.Gateway`) composes all modules
into a single host. The frontend lives in `src/apps/interface` (not part of the `.sln`).
`examples/Ouranos.Pantheon.Examples` is a genetic-algorithm demo in the `.sln` - not a pattern
reference.

When searching the repo, exclude `bin/`, `obj/`, `node_modules/` and `.claude/worktrees/` (full
repo copies) to avoid duplicate hits.

## Commands

### Backend

```bash
dotnet build Ouranos.Pantheon.sln
dotnet test
dotnet test --filter "FullyQualifiedName~Hermes"         # single module
dotnet test --filter "FullyQualifiedName~CreateTraitHandlerTests.Handle_WhenHappyPath"  # single method
```

### Frontend (`src/apps/interface/`)

```bash
npm ci              # install (always use ci, even locally)
npm run dev         # dev server (bound to 0.0.0.0)
npm run build       # also the only type check - there is no typecheck script (or: npx tsc --noEmit)
npm run lint        # eslint src
npm run test        # vitest run (jsdom + Testing Library)
```

Next.js writes its own `src/apps/interface/AGENTS.md` (gitignored) pointing at the bundled Next 16
docs in `node_modules/next/dist/docs/` - read those before using unfamiliar Next.js APIs.

### Formatting

`automation/format.sh` runs CSharpier, `dotnet format style`, `dotnet format analyzers`, then
`eslint --fix` on the frontend. Whitespace is CSharpier's job; never run bare `dotnet format` (its
whitespace pass conflicts with CSharpier), and never pass `--severity` - most `.editorconfig` rules
are warnings and `--verify-no-changes` still fails on them.

### Migrations

`dotnet-ef` is a pinned local tool in `.config/dotnet-tools.json` (alongside CSharpier and
reportgenerator; run `dotnet tool restore` first). Migrations live in each module project
(schema-per-module) and are applied at startup via `ApplyCorePostgresMigrations<TContext>()`.
Each EF context has a design-time factory (built on `OuranosDesignTimeDbContextFactory`) that reads
`appsettings.json` + `appsettings.Development.json`, so the module project is its own
startup project:

```bash
dotnet ef migrations add <Name> \
  --project src/modules/plutus/Ouranos.Pantheon.Modules.Plutus \
  --context PlutusDbContext
```

| Context | Project | Migrations folder |
|---------|---------|-------------------|
| `PlutusDbContext` | `Modules.Plutus` | `Migrations/` |
| `HermesDbContext` | `Modules.Hermes` | `Migrations/` |
| `SharedDbContext` | `Modules.Shared` | `Migrations/Shared/` (pass `--output-dir Migrations/Shared`) |
| `TickerQDbContext` | `Modules.Shared` | `Migrations/TickerQ/` (pass `--output-dir Migrations/TickerQ`) |

Hestia has no EF migrations or design-time factory: `HestiaDbContext` is empty and its data lives
in Marten. Keep EF migrations schema-only and fast - see [Heavy Data Fixes](#heavy-data-fixes).

### Running Locally

- Infrastructure (PostgreSQL with TimescaleDB, RabbitMQ, Flagsmith) is **not** in this repo; use
  [ouranos-infrastructure](https://github.com/davwales/ouranos-infrastructure).
- `appsettings.Development.json` is gitignored and absent from a fresh clone; `appsettings.json`
  lists every key under the `Ouranos:` root.
- The gateway listens on port 8300. Data-loader workers and scheduled jobs are in-process hosted
  services. **Every data loader defaults to enabled** - toggle with
  `Ouranos:Plutus:DataLoaders:<Ffxiv|Osrs|Stocks|Consumer>:IsEnabled`.

### Git Hooks and CI

The `CopyGitHooks` target in `Directory.Build.props` copies `automation/` hooks into `.git/hooks`
after every build. **Pre-commit** checks .NET formatting only (CSharpier + style + analyzers with
`--verify-no-changes`); **pre-push** runs `dotnet build`, no tests.

`.github/workflows/ci.yml` runs on pushes to `main` and all PRs:

- **lint:** CSharpier check, `dotnet format style` / `analyzers --verify-no-changes`, `npm run lint`,
  plus High/Critical audits (`dotnet list package --vulnerable`, `npm audit --omit=dev`)
- **build:** `dotnet build Ouranos.Pantheon.sln`, `npm run build`
- **test:** `dotnet test --collect:"XPlat Code Coverage"` merged with reportgenerator; fails below
  **85% line coverage**. Then `npm run test` (no frontend coverage gate).
- **publish** (`main` only): gateway/interface images to GHCR
  (`ghcr.io/<owner>/ouranos-pantheon/<name>`), tagged `latest` and `sha-<short>`

Actions are pinned by commit SHA with a `# vX.Y.Z` comment; keep that form when adding a step.
`codeql.yml` scans C#, TypeScript and workflows. `dependabot-auto-merge.yml` auto-merges
minor/patch Dependabot PRs.

Images: both Dockerfiles build from the repo root and run as non-root with a `HEALTHCHECK`; the
interface uses Next.js `output: "standalone"`. `.dockerignore` excludes local settings, so a file
the image needs at runtime must not match it.

### Issues and Pull Requests

Write every issue and PR from the platform templates in [`docs/templates/`](docs/templates/README.md):
`issues/feature.md`, `issues/bug.md`, `issues/tech-debt.md` and `pull-request.md`.

- **Issue titles:** `<Module> | <Title>` (`Core`, `Hermes`, `Hestia`, `Plutus`), plus the module
  label and `bug` / `enhancement` where it applies.
- **PR titles:** Conventional Commits, `<type>(<scope>): <summary>`.
- **Keep them minimal:** a few bullets per section, nothing the diff already shows. Older issues and
  PRs are too verbose to use as examples.

## Architecture

### IPantheonModule Composition

Every module implements `IPantheonModule` (`Modules.Shared.Contract/IPantheonModule.cs`):
`Build` and `Configure` are required; `MapEndpoints` and `ConfigureWolverine` default to no-ops.
The gateway's `Startup/HostingExtensions.cs` declares the `Modules` list and passes it to
`AddOuranosCore` / `UseOuranosCore`, which iterate it.

### Vertical Slice Pattern

Each feature is a folder under `Features/` containing:

- `<Name>Handler.cs` - implements `IPantheonHandler<TInput, TOutput>` (or `IPantheonHandler<TInput>`
  for fire-and-forget, or `IPantheonStreamHandler<TInput, TOutput>` for streaming)
- `<Name>Endpoint.cs` - `public static class` with `public static void Map(WebApplication app)` and
  an `internal static` `Handle` method that dispatches via `IMessageBus.InvokeAsync<T>(input)`.
  Exceptions: SSE streaming endpoints write through `SseWriter` from
  `InvokeAsync<IAsyncEnumerable<T>>`, and assistant endpoints just call `app.MapAssistant<TContext>(route)`.
- `Schemas/` - one type per file: `<Name>Input.cs`, plus `<Name>Response.cs` unless a shared
  response type fits (e.g. `IdResponse<T>` for creates). Request bodies (`<Name>Body.cs`) and
  slice-local enums also go here. Prefer `sealed record`.

Wiring gotchas:

- **Endpoints are registered by hand** - add the slice's `Map(app)` call to the module's
  `MapEndpoints`, or the route doesn't exist.
- **Wolverine only discovers handlers implementing the `IPantheonHandler` marker** (all the generic
  interfaces do); anything else is silently ignored.
- **Lambda-factory registrations** (`AddHttpClient<,>(…)` with a configure lambda, DbContexts) need
  `opts.CodeGeneration.AlwaysUseServiceLocationFor<T>()` in the module's `ConfigureWolverine`, or
  `WolverineCodegenValidationTests` fails (see `PlutusModule.cs`).

### Key Abstractions

All in `Ouranos.Pantheon.Modules.Shared.Contract` unless noted.

- **`Id<T>`** (`Domain/Id.cs`) - strongly-typed `readonly record struct` wrapping a string, GUID
  by convention; create with `DatabaseExtensions.CreateId<T>()`
- **`BaseEntity<TId>`** - abstract base with `Id`, `CreatedAt`, `UpdatedAt`, protected `Update()`;
  **`BaseEventSourcedEntity`** is the Marten counterpart (an abstract record, Hestia)
- **`OuranosDbContext`** (`Infra/Postgres/`) - abstract base with schema-per-module (e.g.
  `"hermes"`, `"plutus"`), snake_case via `EFCore.NamingConventions`
- **`PostgresModule`** (`Infra/Postgres/PostgresModule.cs`) - `AddCorePostgresModule<TContext>()`,
  `ApplyCorePostgresMigrations<TContext>()`, and `AddCoreMartenModule<TMarker>()` (Hestia only)
- **`QueryOptions`** (`Application/Common/QueryOptions.cs`, config `Ouranos:Query`) - server-side
  paging limits for list endpoints: page size 1-100, max skip 10000
- **`ModelDefaults`** (`Infra/OuranosMachineLearning/ModelDefaults.cs`) - the one place for model
  name, reasoning effort and token budgets; feature options initialize from it and only override

### Data Flow

- **Market data:** external APIs → data-loader workers (WebSocket/HTTP) → RabbitMQ (`TradeMessage`)
  → consumer worker → PostgreSQL/TimescaleDB → REST API → Next.js frontend
- **Long-running jobs over RabbitMQ:** Plutus `RunBacktestMessage` / `OptimizeStrategyMessage`
  (endpoints return 202 Accepted with a `backtestId`) and Hestia `ImportRecipeRequested`
- **Scheduled jobs:** TickerQ (Shared, Hermes, Plutus)

### Infrastructure Dependencies

- **PostgreSQL** - EF Core everywhere; **TimescaleDB** hypertables / continuous aggregates for
  Plutus trades and **Marten** for Hestia (ADR 0004)
- **RabbitMQ** - via Wolverine
- **Flagsmith** - feature flags
- **OuranosMl** - external ML inference service (OpenAI-compatible Responses API)
- **TickerQ** - scheduled jobs
- **`MongoDB.Bson`** - used only by the FFXIV data loader to decode the Universalis WebSocket feed.
  There is no MongoDB server.

### AI Assistants

Page-scoped assistants (Plutus market/symbol analysts, Hestia kitchen assistant) and Hermes chat
share the tool loop in `Shared.Contract/Application/Assistants/`.

- **Layout:** each module keeps one tool catalogue at `Features/Assistants/Tools/<Tool>/` (records
  in `Tools/<Tool>/Schemas/`) and its assistants in `Features/Assistants/Ask<X>/`. Keep the `Ask`
  prefix - a folder named after the class collides with the type (CS0118).
- **Tools** are typed to the narrowest scope interface they need (`Features/Assistants/Scopes/`),
  not to one assistant's context; assistants opt in with `AddAssistantTool<TContext, TTool>()`.
  Tool `*Data` records are compact projections of feature responses, not the responses themselves.
- **Prompts are small:** the model's context window is limited, so prompts contain only what the
  page shows, without GUIDs, with capped rows. Tests assert the worst case against
  `AssistantPromptBudget` (`tests/Ouranos.Pantheon.Tests.Utils/`). In docs, call the window
  "limited" - never cite a token count.
- **Fix LLM behaviour through the prompt** (rules plus few-shot examples), never with hard-coded
  word lists or vocabulary maps in code.
- **Long ouranos-ml calls stream** (`ResponseHeadersRead` + SSE, use the final event) rather than
  raising `HttpClient` timeouts.

## Code Style (Non-Obvious Rules)

### Cross-Cutting

- **Reuse existing mechanisms** before adding a new store, context, registry or abstraction.
- **Typed contracts only** - no `JsonElement` / dictionary payloads; shared extension points take
  generic typed inputs bound at endpoint registration so they appear in OpenAPI.
- **Plutus is market-agnostic** - code, API fields and UI say symbol (not item), order / order
  limit (not offer / GE slots), buy-side / sell-side price, currency units (not gp). Market-specific
  rules go behind `MarketRules`, not hard-coded constants.

### .NET

- **File-scoped namespaces required** (`csharp_style_namespace_declarations = file_scoped:warning`)
- **Always use braces** even for single-line statements (`csharp_prefer_braces = true:error` - the
  only error-severity rule)
- **Classes `sealed` by default** unless designed for inheritance (`CA1852:warning`)
- **Null-guard all injected dependencies** with `Ardalis.GuardClauses` (referenced globally via
  `Directory.Build.props`) - either in a constructor body or as primary-constructor field
  initializers (`private readonly IFoo _foo = Guard.Against.Null(foo);`)
- **⛔ Never use the null-forgiving operator (`!`) in production code** - add an explicit null
  check (`Guard.Against.Null`, `if (x is null)`) or use `??`. The `!` suppresses the compiler
  warning without guaranteeing non-null, so it hides real bugs. Exception: `null!` in test code
  when intentionally testing guard clauses.
- **Handler entry:** `LogTrace` first, then `cancellationToken.ThrowIfCancellationRequested()`,
  and `LogDebug` on success. Use structured message templates with `'{paramName}'`.
- **No expression-bodied methods** (`csharp_style_expression_bodied_methods = false:warning`)
- **Unnecessary usings fail CI** (`IDE0005:warning`)
- **Accessibility keywords required** (`dotnet_style_require_accessibility_keywords = always:warning`)
- **One type per file** - each schema, record, enum, or class gets its own file named after the
  type. Never put multiple types in a single `*Schemas.cs` file.
- XML doc generation is enabled but CS1591 (missing docs) is silent.
- Package versions live only in `Directory.Packages.props` (central management, transitive pinning).

### TypeScript / Frontend

Some rules below are newer than parts of the codebase. **Rules marked (new code) apply to new or
touched code**; existing violations are known and are not a licence to copy the pattern.

- Use `type` for type definitions, not `interface` **(new code)**
- Use `import { type X }` for type-only imports
- **No HTML entities in JSX** (`&ldquo;`, `&rdquo;`, `&apos;`, `&quot;`, …) - write the literal
  character instead (“ ” ’). If `react/no-unescaped-entities` flags a straight `'` or `"`, use the
  typographic character or a string expression (`{"..."}`)
- Prefer named exports over default exports (page components are the exception: `export default`)
- File naming: kebab-case. Path alias: `@/*` → `./src/*`
- **API access:** per-module functions in `src/lib/api/<module>.ts` (types in
  `src/lib/api/<module>-types.ts`), built on the `api` client and `streamSse` from
  `src/lib/api-client.ts`. Components fetch via the `useApi` hook (`src/hooks/use-api.ts`).
- State management: Zustand (stores in `src/stores/`, e.g. `plutus-store.ts`, `breadcrumb-store.ts`)
- **`"use client"` only on components that directly call React hooks or browser APIs** - any React
  hook (state, effects, context, router, `useApi`, `usePlutusStore`, …), event handlers like
  `onClick`/`onChange`, or browser-only globals. Pure presentational components that receive props
  and render JSX do NOT need `"use client"` - the parent that owns the state provides it.
- **Extract components instead of comment blocks** - `{/* Header */}`, `{/* Stats Grid */}` etc.
  are a code smell; replace them with named components (e.g. `<StrategyHeader>`). Target: page
  files under ~200 lines, composing extracted components.
- **Component file organization** - feature-specific components go in a `_components/` subfolder
  next to the pages that use them (e.g. `strategies/_components/run-backtest-dialog.tsx`). Hermes
  pages use a plain `components/` folder, and Plutus has cross-page `plutus/components/`,
  `plutus/views/`, `plutus/constants/`. Shared UI primitives go in `src/components/` (e.g.
  `components/shared/responsive-dialog/`). Domain-specific constants (labels, icon mappings) go
  in a `*-constants.ts(x)` file in the same components folder.
- **Component naming conventions** - file suffixes name the visual role:
  - `*-view.tsx` → `XxxView` - self-contained content blocks that fetch their own data and render a
    distinct area of the page (e.g. `ForecastEfficacyView`, `FailedBacktestView`)
  - `*-card.tsx` → `XxxCard` (e.g. `SignalCard`, `StatCard`)
  - `*-dialog.tsx` → `XxxDialog` (e.g. `RunBacktestDialog`)
  - `*-form.tsx` → `XxxForm` (e.g. `StrategyConfigForm`)
  - `*-grid.tsx` → `XxxGrid` - grid/flex layouts of repeated items (e.g. `BacktestMetricsGrid`)
  - `*-table.tsx` → `XxxTable` (e.g. `BacktestPositionsTable`)
  - `*-header.tsx` → `XxxHeader` (e.g. `StrategyHeader`)
  - `*-panel.tsx` → `XxxPanel` - sidebar or collapsible panels (e.g. `RecommendationsPanel`)
  - `*-chart.tsx` → `XxxChart` (e.g. `PriceChart`)
  - **Do NOT use `*-section.tsx` or `*-component.tsx` (new code)** - too vague. Rename legacy
    `*-section.tsx` files when you touch them.
- **Breadcrumbs are owned by routes** - the global bar labels static segments from the title-cased
  folder name and links every ancestor, so every URL prefix must resolve (add a redirect `page.tsx`
  if a parent has no page, e.g. `hestia/page.tsx` → `/hestia/recipes`). GUID segments are labelled
  by whichever page/layout already has the entity: call `useBreadcrumbLabel(id, data?.name)` next to
  the existing `useApi`, or render `<BreadcrumbLabel segment={id} label={name} />` from a server
  layout whose children need it (both in `src/components/shared/breadcrumbs/`). Never fetch just for
  a breadcrumb; an unregistered GUID renders as a skeleton.
- **Assistants live in the top bar** - a page that offers an AI assistant renders a button built on
  the shared `AssistantButton` (`components/shared/ai-assistant/`) through
  `useNavBarActions().setActions` inside an effect, with `clearActions` as cleanup (combine it with
  any other nav-bar actions the page sets). Only set it while the assistant is usable, and key it by
  the context id so the conversation resets per entity. Never place assistant triggers in page
  content.
- **Editors guard unsaved changes** - a form that edits an entity calls
  `useUnsavedChangesGuard(isDirty)` (`src/hooks/`) with `hasChanges(current, baseline)`
  (`src/lib/has-changes.ts`). Keep the baseline as the last saved values, not the mount-time
  state, when the page stays mounted after saving.
- **Global navigation is data** - nav-bar entries live in `NAVIGATION_ITEMS`
  (`components/shared/responsive-navigation-bar/navigation-items.ts`) and per-market pages in
  `MARKET_FEATURES` (`plutus/constants/market-features.ts`); the command palette
  (`app/_components/command-palette/`) builds its pages from both, so add new pages there.
- **Backtest/Optimization UI pattern** - trigger dialogs from the strategy detail page
  (`RunBacktestDialog`, `OptimizeDialog`), both using `ResponsiveDialog`. After submission
  (HTTP 202 Accepted), navigate to `/plutus/[marketId]/strategies/[strategyId]/backtests`. Backtest
  detail (`.../backtests/[backtestId]`) shows status-dependent views (Pending/Running with spinner,
  Failed with error, Cancelled, Completed with results dashboard).

## Bruno API Collection

`bruno/ouranos-pantheon/collections/API/` holds one YAML request file per endpoint, organized by
module (`Plutus/`, `Hermes/`, `Hestia/`, `Health/`), with URLs built from environment variables.
Keep it covering every mapped route.

**⛔ Any endpoint change (new/renamed/removed route, changed query params, or changed
request/response schemas) must update the corresponding request file(s) in the same change.**

- **Query params MUST be in the URL** - append them to `http.url` (e.g.
  `.../positions?marketId={{MARKET_ID}}`) and mirror them in the `http.params` list. Params defined
  only in the `http.params` list are NOT sent.
- **Path segments interpolate with double braces only** - `{{POSITION_ID}}`, never `{POSITION_ID}`.
- **New env vars go in BOTH** `environments/local.yml` and `environments/production.yml` (same
  variable names; they differ only in `BASE_URL`).
- Query param names match the C# input record properties (bound case-insensitively), e.g.
  `GetAllPositionsInput.MarketId` → `marketId`.

## Architecture Documentation (arc42 + ADRs)

Architecture is documented per [arc42](https://arc42.org) with [MADR](https://adr.github.io/madr/)
decision records, federated over two levels (ADR 0013):

- **Platform** - `docs/architecture/` (one file per section, indexed by its `README.md`) and
  `docs/adr/` (indexed by its `README.md`). Describes modules only as blackboxes.
- **Module** - `src/modules/<module>/docs/`: a required `README.md` (blackbox: purpose, context,
  interfaces, messages, jobs, persistence, configuration, index) plus, once they have content,
  `05-building-block-view.md`, `06-runtime-view.md` (large scenarios in `runtime/<name>.md`),
  `08-concepts.md`, `11-risks-and-technical-debt.md` (IDs `<MODULE>-D<n>`), `12-glossary.md`,
  and `adr/` (cited `<MODULE>-ADR-NNNN`). New modules copy `docs/templates/module-docs/`.

**⛔ Any architectural change must update the docs at the level that owns it, in the same change.**
Keep Mermaid diagrams in sync with the code.

- **Platform (`docs/`) only when** the change alters the module contract or a Section 8 convention,
  adds host infrastructure, adds a message/call/integration crossing module boundaries,
  adds/renames/removes a module, or changes deployment, CI, quality goals or constraints.
- **Otherwise the module's docs**: slices, module-internal messages and queues, scheduled jobs,
  `Ouranos:<Module>` config keys, an external API only that module consumes, assistants and tools,
  module debt and vocabulary. A module change should not touch `docs/`.
- **Links:** module docs may link anywhere in the platform docs; platform docs link to a module only
  through its `docs/README.md`, never into its other files or anchors.
- **State each fact once**, at the lowest level that owns it.

Add an ADR only for major decisions with lasting consequences and real alternatives (e.g. choosing a
message broker). A decision confined to one module goes in that module's `adr/` log and its index
only; a platform ADR is also listed in `docs/adr/README.md` and
`docs/architecture/09-architecture-decisions.md`. A module ADR never contradicts a platform ADR.
Refactors that align code with established patterns, including in-module restructures, do not need
an ADR.

## Testing

Stack: xUnit **v2**, AutoFixture, NSubstitute, Shouldly, EF Core InMemory, coverlet. All tests are
unit tests - no Testcontainers or `WebApplicationFactory`. `Apps.Gateway.Tests` includes
`WolverineCodegenValidationTests`, which builds the real host and fails if a handler cannot be
code-generated by Wolverine.

### Conventions

- **One test file per source file**, at the mirrored path under `tests/`
  (`src/.../GetSignalsTool.cs` → `tests/.../GetSignalsToolTests.cs`). Never combine several types
  in one test class. Include constructor null-guard tests.
- **Naming:** `<Method>_When<Scenario>_Should<ExpectedBehavior>`, or `<Method>_Should<ExpectedBehavior>`
  when there is no meaningful scenario. Use `ShouldThrowAsync<T>()` for exception tests.
- **Structure:** every test has `// Arrange`, `// Act`, `// Assert` markers, even simple ones.
  `// Act & Assert` is acceptable when the act is the assertion (e.g. `ShouldThrowAsync`).
- **Layout mirrors `src/`:** `tests/modules/{hermes,plutus,hestia,shared}`, `tests/apps/`. Each
  test project has its own `coverage.runsettings`.
- **Frontend:** vitest files (`*.test.ts` / `*.test.tsx`) are colocated with the code.

### Test Utilities (`tests/Ouranos.Pantheon.Tests.Utils/`)

Check here before writing a helper. The most used:

- `DbContextExtensions.Mock<TContext>()` / `MockFactory<TContext>()` /
  `SeedData<TContext, TEntity>()` - in-memory DB context, `IDbContextFactory`, and seeding
- `ObjectExtensions.WithCreatedAt<TEntity>()` / `Protected()` - set `CreatedAt`; invoke non-public
  virtual members on NSubstitute proxies
- `IdCustomization` - AutoFixture customization for `Id<T>`
- `ArgExtensions.IsEquivalent<T>()` - NSubstitute deep structural equality matcher
- `ShouldContainService<T>()` / `ShouldContainServiceWithImplementation<T, TImpl>()` - DI
  registration assertions
- `AsyncEnumerableValidations.ShouldMatchAsync<T>()` - element-by-element async enumerable comparison

## Adding a Feature (End-to-End)

1. Start from the latest trunk: `git fetch origin` and branch off `origin/main` (never a stale
   local `main` or another feature branch)
2. Create the slice folder under `Features/` (Handler, Endpoint, `Schemas/` with one type per file)
3. Add the endpoint's `Map` call to the module's `MapEndpoints`, and register anything else
   module-level in its `IPantheonModule` implementation
4. Add or update the Bruno request file(s) for any endpoint change
5. Add tests under `tests/`, one test file per source file
6. If the change is architectural, update the module's docs (`src/modules/<module>/docs/`), or the
   platform docs if it crosses the module boundary (ADR only for major decisions)
7. Run `automation/format.sh`, then `dotnet build` and `dotnet test` (plus `npm run lint` /
   `npm run test` for frontend changes)

## Heavy Data Fixes

One-off corrections touching hundreds of thousands of rows or more are run manually, per
environment - never through `ApplyCorePostgresMigrations`. Startup blocks while the rewrite runs and
the 30s command timeout kills it mid-flight, crash-looping the host. Write them as idempotent SQL
scripts (not checked in), guard for idempotency, drop/rebuild any indexes the script touches, and
hand them to the user to run.
