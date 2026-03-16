# ABBA Fleet — Claude Instructions

## Running locally
```
docker compose up
```

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
  Pages/Account/           — Razor Pages for login/logout (outside Blazor component tree)
docs/adr/                  — Architecture Decision Records
```

Note: `Infrastructure/` and `Pages/Account/` are not present in the initial scaffold — they are added as features require them.

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

## Constraints
- **Host SDK:** Host machine has .NET 9 — `dotnet ef` cannot run on the host. Use the `/new-migration` command instead.
- **File deletion:** `rm -rf` is denied in user settings — ask the user to manually delete folders when needed.

## Slash commands
If the user asks to do something that matches a slash command below, invoke that command unless they explicitly say not to.

- `/new-migration` — add an EF Core migration
- `/new-feature` — scaffold a new vertical slice feature folder
- `/create-pr` — create a pull request with Linear ticket linked
