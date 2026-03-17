---
name: new-feature
description: Scaffold a new vertical slice feature folder. Use this when the user asks to create a new feature, page, or section of the app. Requires the feature name as the argument.
allowed-tools: Bash, Write
---

Scaffold a new vertical slice feature folder for the feature name provided as the argument.

1. Create the folder `src/app/Features/$ARGUMENTS/`
2. Create a placeholder Razor component `src/app/Features/$ARGUMENTS/$ARGUMENTS.razor` with:
   - `@page "/route-here"` (derive a sensible kebab-case route from the feature name)
   - `<PageTitle>$ARGUMENTS</PageTitle>`
   - `<h1>$ARGUMENTS</h1>`
3. Tell the user what was created and what the route is.

Do not create services, models, or other files — those get added when the feature is actually implemented.
