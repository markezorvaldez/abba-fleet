# ADR-002: Authentication and File Storage

Status: Accepted
Date: 2026-03-15

## Context

The app has ~5 internal users. There is no self-registration — accounts are created
by an admin. Some actions are restricted by role. Roles need to be supported but the
full list is not yet defined.

Files are uploaded throughout the app — vouchers, receipts, investment documents —
and need to be stored reliably outside the server (Railway deployments are ephemeral).

## Decision

**Authentication**
- ASP.NET Core Identity with cookie-based authentication
- Login and logout handled via Razor Pages (outside the Blazor component tree)
- Passwords hashed by Identity
- Roles stored in the Identity database, enforced via [Authorize(Roles = "...")] attributes

**File storage**
- Supabase Storage (free tier, 1GB)
- Files uploaded via the Supabase REST API from the .NET backend
- Private buckets with signed URLs for sensitive documents

**Database for auth**
- Identity tables stored in the same Railway PostgreSQL database as the rest of the app

## Reasons

- ASP.NET Core Identity is the standard .NET auth solution — integrates directly
  with EF Core and Blazor Server
- Cookie-based auth is natural for Blazor Server — no JWT tokens needed across a wire
- Supabase Storage is free, reliable, and avoids the ephemeral storage problem on Railway
- Keeping auth in the app database avoids an external auth dependency while still
  using a proven framework

## Consequences

- User management (create, reset password) must be built — no self-service UI from Identity
- Supabase free tier pauses after 1 week of inactivity — file access will be slow
  on first request after a quiet period
- Roles must be seeded manually or via an admin UI
