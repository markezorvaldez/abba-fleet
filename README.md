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
  app/              ← Blazor Server project
    Features/       ← one folder per feature (vertical slice)
    Infrastructure/ ← cross-cutting concerns (DbContext, Identity, migrations)
    Components/     ← shared layout components
    Pages/          ← Razor Pages (login/logout)
docs/
  adr/              ← Architecture Decision Records
```

## Running locally

Prerequisites: [Docker Desktop](https://www.docker.com/products/docker-desktop/)

```bash
docker compose up
```

App: http://localhost:5000

## Architecture decisions

See [docs/adr/](docs/adr/) for records of key architectural decisions and their reasoning.
