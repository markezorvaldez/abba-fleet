# ABBA Fleet Management System

Internal web-based admin platform for ABBA, a trucking company based in the Philippines. Manages fleet operations including trip tracking, expense logging, payment reconciliation, and financial reporting.

## Stack

- **App:** Blazor Server (.NET 10)
- **ORM:** EF Core
- **Database:** PostgreSQL
- **Local dev:** Docker Compose

## Repository structure

```
src/
  app/                            ← Blazor Server project
    Features/                     ← one folder per feature (vertical slice)
    Shared/                       ← types shared across all layers (Permission, AppRoutes, service interfaces)
    Infrastructure/               ← implementations wired to framework concerns (DbContext, Identity, migrations)
    Components/                   ← shared layout components
tests/
  AbbaFleet.Unit.Tests/           ← domain logic and business rule tests
  AbbaFleet.Integration.Tests/    ← HTTP, auth, and DB connectivity tests
  AbbaFleet.Architectural.Tests/  ← ArchUnitNET layer enforcement tests
docs/
  adr/                            ← Architecture Decision Records
```

## Running locally

Prerequisites: [Docker Desktop](https://www.docker.com/products/docker-desktop/)

```bash
docker compose up
```

App: http://localhost:5000

## Code style

Code style is enforced by `dotnet format`, which reads `.editorconfig`:

```bash
dotnet format abba-fleet.sln
```

If a PR fails the format check, run the command above to automatically fix violations, then commit and push.

## Architecture decisions

See [docs/adr/](docs/adr/) for records of key architectural decisions and their reasoning.
