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

### Pull request descriptions

If a PR is already open and further changes are made, update the PR description to reflect the current state of the work. It should always accurately describe what the PR contains.

### Testing after code changes

After implementing any code changes, always test before considering the work done. Use judgement on the appropriate level:

- **Build verification** — always run `docker compose build` and confirm no errors
- **Unit tests** — for business logic, calculations, and validations; run with `dotnet test`
- **Playwright** — for any UI changes, use the Playwright MCP tools to verify the affected flows work correctly in the browser. Save screenshots to `C:/Repositories/screenshots/abba-fleet/<TICKET-ID>/` (e.g. `01-login-page.png`, `02-dashboard.png`)
- **Integration** — for behaviour that touches the database or auth, verify end-to-end in the running app

## Constraints
- **Host SDK:** Host machine has .NET 9 — `dotnet ef` cannot run on the host. Use the `/new-migration` skill instead.
- **File deletion:** `rm -rf` is denied in user settings — ask the user to manually delete folders when needed.
