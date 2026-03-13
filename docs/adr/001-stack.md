# ADR-001: Application Stack and Hosting

Status: Accepted
Date: 2026-03-12

## Context

Internal fleet management web app for a small team (~5 users) in the Philippines.
Solo developer with strong C# backend background and minimal frontend experience.
Future contributors from the same family may join — easy onboarding matters.

## Decision

**Stack**
- Backend: ASP.NET Core Web API (.NET 10)
- Frontend: React + TypeScript (Vite)
- ORM: EF Core
- Database: PostgreSQL

**Local development**
- Docker Compose runs all three services together
- Anyone with Docker Desktop can clone and run with a single command

**Hosting**
- API: Railway (free tier, auto-deploys from main branch)
- Database: Railway PostgreSQL (built-in, same platform)
- Frontend: Vercel (free tier, auto-deploys from main branch)

## Reasons

- .NET 10 is the latest release; no reason to use an older version on a personal project
- React is the industry standard frontend framework with the most transferable skills
- Docker Compose makes onboarding trivial — one command, no prerequisite installs beyond Docker
- Railway + Vercel provide free hosting with automatic GitHub-based deploys and no cold start issues

## Consequences

- Docker Desktop must be installed locally
- Future contributors only need Docker Desktop to get started
- Production uses Railway/Vercel rather than Docker — local and production environments differ slightly
