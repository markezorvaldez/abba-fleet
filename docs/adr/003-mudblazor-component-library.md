# ADR 003 — MudBlazor as UI Component Library

## Status
Accepted

## Context
The app needed a UI component library to avoid building every table, dialog, form, and layout from scratch. A Blazor-native library was preferred over a generic CSS framework, as it provides pre-built interactive components (data tables, dialogs, snackbars, chips) that integrate naturally with the Blazor component model and server-side state.

## Decision
Use [MudBlazor](https://mudblazor.com/) as the primary UI component library. MudBlazor implements Material Design and provides a rich set of components — `MudTable`, `MudDialog`, `MudTextField`, `MudCheckBox`, `MudSnackbar`, etc. — that map directly to the UI patterns this app needs.

MudBlazor is added as a NuGet package. The CSS and JS are served from the package's static assets (`_content/MudBlazor/`). The required providers (`MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`, `MudSnackbarProvider`) are added to `MainLayout.razor`.

## Consequences
- All new features use MudBlazor components as the default — no custom HTML tables or dialogs unless MudBlazor lacks the component needed
- The Roboto font is loaded from Google Fonts — acceptable for an internal tool on a stable connection; can be self-hosted if needed
- MudBlazor's Material Design aesthetic is the visual baseline for the entire app going forward
