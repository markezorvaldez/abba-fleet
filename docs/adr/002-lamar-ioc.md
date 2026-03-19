# ADR 002 — Lamar as IoC Container

## Status
Accepted

## Context
As the app grows, manually registering every service in `Program.cs` with `builder.Services.AddScoped<IFoo, Foo>()` becomes repetitive boilerplate. We needed an IoC container that supports convention-based auto-discovery so that services following a standard `IFoo → Foo` naming pattern are registered automatically.

## Decision
Use [Lamar](https://jasperfx.github.io/lamar/) as a drop-in replacement for the built-in Microsoft DI container. Lamar is configured to scan the calling assembly and apply default conventions — any class `Foo` that implements `IFoo` is registered automatically.

Existing registrations made via `builder.Services.*` (Identity, EF Core, MudBlazor, etc.) are preserved; Lamar picks them up unchanged.

Services that require a specific lifetime (e.g. `Scoped` for circuit-bound services) are registered explicitly in the Lamar registry.

## Consequences
- New services following `IFoo → Foo` require zero registration code
- Services with special lifetime or constructor requirements are registered explicitly in `Program.cs`
- Lamar replaces the default MS DI `IServiceProvider` — edge cases with deeply nested `IServiceProviderFactory` usage may need attention, but none exist in this codebase
