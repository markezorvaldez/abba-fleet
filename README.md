# ABBA Fleet Management System

Internal web-based admin platform for ABBA, a trucking company based in the Philippines. Manages fleet operations including trip tracking, expense logging, payment reconciliation, and financial reporting.

## Stack

- **Backend:** ASP.NET Core Web API (.NET 10)
- **Frontend:** React + TypeScript (Vite)
- **ORM:** EF Core
- **Database:** PostgreSQL
- **Local dev:** Docker Compose

## Repository structure

```
src/
  api/    ← ASP.NET Core Web API
  web/    ← React frontend
docs/
  adr/    ← Architecture Decision Records
```

## Running locally

Prerequisites: [Docker Desktop](https://www.docker.com/products/docker-desktop/)

```bash
docker compose up
```

- API: http://localhost:5000
- Frontend: http://localhost:5173

## Architecture decisions

See [docs/adr/](docs/adr/) for records of key architectural decisions and their reasoning.
