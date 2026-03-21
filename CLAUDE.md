# ABBA Fleet — Claude Instructions

## Running locally
```
docker compose up --build
```

## Project structure
```
src/app/                   — single Blazor Server project (.NET 10)
  Features/                — one folder per feature (vertical slice)
  Shared/                  — types shared across all layers (Permission, AppRoutes, service interfaces)
  Infrastructure/          — implementations wired to framework concerns (DbContext, Identity, migrations)
  Components/Layout/       — shared layout components
tests/
  AbbaFleet.Unit.Tests/           — domain logic and business rule tests
  AbbaFleet.Integration.Tests/    — HTTP, auth, and DB connectivity tests
  AbbaFleet.Architectural.Tests/  — ArchUnitNET layer enforcement tests
docs/adr/                  — Architecture Decision Records
```

## Conventions

### Vertical slice
Each feature lives in `Features/FeatureName/`. Don't scatter feature code into shared folders — only truly cross-cutting types belong in `Shared/`, and only framework-coupled implementations belong in `Infrastructure/`.

### Layer boundaries (enforced by ArchUnitNET)
- Features must not reference other features
- Components must not reference Features or Infrastructure (use Shared instead)
- Infrastructure must not reference Features
- Razor components must not reference DbContext or EF Core directly

### ADRs
When a significant architectural or technology decision is made, write an ADR in `docs/adr/` using the next available number. Capture the decision, the alternatives considered, and the reason.

### Branching
Branch naming: `feature/mar-XX-short-description`

### Commits
Short imperative subject line, with the Linear ticket referenced naturally in the description. Always add the co-author trailer:
```
Co-Authored-By: Claude <noreply@anthropic.com>
```

### Committing changes

After implementing and testing, stop and let the user review the changes in their source control tool before doing any git operations. Never stage or commit as part of implementing a feature — only do so when the user explicitly asks to commit or sync.

### Pull request descriptions

If a PR is already open and further changes are made, update the PR description to reflect the current state of the work. It should always accurately describe what the PR contains.

### Testing after code changes

After implementing any code changes, always write and run the appropriate tests before considering the work done:

- **Unit tests** (`tests/AbbaFleet.Unit.Tests/`) — domain logic and pure business rules. Add when introducing new domain methods. Mirror the source folder structure. Run with `dotnet test tests/AbbaFleet.Unit.Tests`.
- **Integration tests** (`tests/AbbaFleet.Integration.Tests/`) — HTTP, auth, and DB connectivity. Test wiring, not every rule. Only add when there is meaningful connectivity to verify. Run with `dotnet test tests/AbbaFleet.Integration.Tests`.
- **Architectural tests** (`tests/AbbaFleet.Architectural.Tests/`) — ArchUnitNET layer enforcement. Run with `dotnet test tests/AbbaFleet.Architectural.Tests`. Add rules when new layer boundaries are introduced.
- **Playwright** — for UI changes, verify affected flows in the browser. Save screenshots to `C:/Repositories/screenshots/abba-fleet/<TICKET-ID>/`.

Run the full suite with `dotnet test`.

### Unit test style

- **Tight mock arrange on positive tests:** Use `Arg.Is<T>(predicate)` matching expected values, not `Arg.Any<T>()`. Loose matchers are fine for negative/failure-path tests.
- **Use AutoFixture for test data:** Use `IFixture` / `_fixture.Create<T>()` for arbitrary test values rather than hardcoding magic strings and numbers.

### EditorConfig compliance

After writing or editing any C# file, run `dotnet format abba-fleet.sln --verify-no-changes` and fix violations before committing.

### Proactive tech debt

When implementing a feature, always include a brief "Known shortcuts / tech debt" note in the plan if any validation, security, or concurrency gaps are knowingly deferred. Don't wait to be asked — surface these during planning.