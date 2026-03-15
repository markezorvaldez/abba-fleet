# ADR-001: Application Stack and Hosting

Status: Accepted
Date: 2026-03-15

## Context

Internal fleet management web app for a small team (~5 users) in the Philippines.
Solo developer with strong C# backend background and minimal frontend experience.
A family member repo exists on the same stack — comparison and potential future
collaboration are considerations.

## Decision

**Stack**
- Framework: ASP.NET Core Blazor Server (.NET 10)
- Architecture: Vertical Slice (single project)
- ORM: EF Core
- Database: PostgreSQL

**Local development**
- Docker Compose runs the app and PostgreSQL together
- Anyone with Docker Desktop can clone and run with a single command

**Hosting**
- Full app (Blazor Server): Railway (free tier, auto-deploys from main branch)
- Database: Railway PostgreSQL (built-in, same platform)

## Reasons

- Blazor Server stays in C# end-to-end — no context switch to a frontend language
- Comparable to the family repo, enabling meaningful side-by-side comparison
- Vertical Slice organises code by feature rather than technical layer — natural fit
  for a CRUD-heavy admin tool where each feature is self-contained
- Single project keeps complexity low without sacrificing structure
- Railway hosts the full app in one service — no separate frontend hosting needed
- Docker Compose makes onboarding trivial — one command, no prerequisites beyond Docker

## Consequences

- Blazor Server requires a persistent server connection (SignalR) — not suitable
  for static hosting
- All hosting is on Railway — no Vercel needed
- Future contributors need .NET knowledge to work on any part of the app
