# ABBA Fleet — Claude Instructions

## Running locally
```
docker compose up --build
```
Always use `--build` — source files are copied into the image at build time, so without it the container runs stale code. Remind the user to run `docker compose up --build` after every set of code changes.

## Verifying the build
```
docker compose build
```
Always run this after code changes and confirm `Build succeeded. 0 Error(s)` before presenting results.

## Project structure
```
src/app/                   — single Blazor Server project (.NET 10)
  Features/                — one folder per feature (vertical slice)
  Infrastructure/          — cross-cutting concerns (DbContext, Identity, migrations)
  Components/Layout/       — shared layout components
docs/adr/                  — Architecture Decision Records
```

Note: `Infrastructure/` is not present in the initial scaffold — it is added as features require it.

## Conventions

### Vertical slice
Each feature lives in `Features/FeatureName/`. Don't scatter feature code into shared folders — only truly cross-cutting concerns belong in `Infrastructure/` or `Components/`.

### ADRs
When a significant architectural or technology decision is made, write an ADR in `docs/adr/` using the next available number. Capture the decision, the alternatives considered, and the reason.

### Branching
Branch naming: `feature/mar-XX-short-description`

### Commits
Short imperative subject line, with the Linear ticket referenced naturally in the description. Always add the co-author trailer:
```
Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
```

### Committing changes

After implementing and testing, stop and let the user review the changes in their source control tool before doing any git operations. Never stage or commit as part of implementing a feature — only do so when the user explicitly asks to commit or sync.

### Pull request descriptions

If a PR is already open and further changes are made, update the PR description to reflect the current state of the work. It should always accurately describe what the PR contains.

### Testing after code changes

After implementing any code changes, always write and run the appropriate tests before considering the work done:

- **Build verification** — always run `docker compose build` and confirm no errors
- **Unit tests** (`tests/AbbaFleet.UnitTests/`) — for domain logic and pure business rules with no external dependencies. Always add when introducing new domain methods or business rules. Mirror the source folder structure (e.g. `Domain/`, `Features/`). Run with `dotnet test tests/AbbaFleet.UnitTests`.
- **Integration tests** (`tests/AbbaFleet.IntegrationTests/`) — for HTTP, auth, and database connectivity. Test that the wiring works — not every business rule. Only add when there is meaningful connectivity to verify (e.g. a new auth flow, a new endpoint). Run with `dotnet test tests/AbbaFleet.IntegrationTests`.
- **Playwright** — for any UI changes, use the Playwright MCP tools to verify the affected flows work correctly in the browser. Save screenshots to `C:/Repositories/screenshots/abba-fleet/<TICKET-ID>/` (e.g. `01-login-page.png`, `02-dashboard.png`)

Run the full suite with `dotnet test`.

### EditorConfig compliance

All C# code must follow `.editorconfig`. Key rules to always apply:
- **Braces required** (`csharp_prefer_braces = true:warning`, `IDE0011`) — every `if`, `else if`, and `else` body must use `{ }`, including single-line guard clauses and early returns
- **`var` everywhere** — use `var` for all local variable declarations
- **File-scoped namespaces** — `namespace Foo;` not `namespace Foo { }`

After writing or editing any C# file (including test files), always run:
```
dotnet format abba-fleet.sln --verify-no-changes
```
Fix any reported violations before committing. The CI format check will fail on the same issues.

### Proactive tech debt

When implementing a feature, always include a brief "Known shortcuts / tech debt" note in the plan if any validation, security, or concurrency gaps are knowingly deferred. Don't wait to be asked — surface these during planning.

## Constraints
- **Host SDK:** Host machine has .NET 9 — `dotnet ef` cannot run on the host. Use the `/new-migration` skill instead.
- **File deletion:** `rm -rf` is denied in user settings — ask the user to manually delete folders when needed.
