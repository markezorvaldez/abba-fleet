---
name: new-migration
description: Add a new EF Core migration. Use this when the user asks to add a migration or when schema changes need to be persisted. Requires a migration name as the argument.
---

Add a new EF Core migration with the name provided as the argument.

Run the following command, substituting $ARGUMENTS for the migration name:

```
docker run --rm -v "C:/Projects/Interviews/abba-fleet/src/app:/app" mcr.microsoft.com/dotnet/sdk:10.0 bash -c "cd /app && dotnet tool install --global dotnet-ef && export PATH=\"$PATH:/root/.dotnet/tools\" && dotnet ef migrations add $ARGUMENTS --output-dir Infrastructure/Data/Migrations"
```

After it completes, confirm the migration file was created under `src/app/Infrastructure/Data/Migrations/` and show the user the generated file names.
