---
name: add-validation
description: Scaffold a FluentValidation validator class for a feature. Use this when the user asks to add validation to a feature or model. Requires the feature name as the argument.
---

Scaffold a FluentValidation validator for the feature name provided as the argument.

1. Identify the request/model type that needs validating in `src/app/Features/$ARGUMENTS/` (typically named `Upsert$ARGUMENTSRequest` or similar — read the feature folder to confirm).
2. Create `src/app/Features/$ARGUMENTS/$ARGUMENTSValidator.cs` with:
   - Namespace `AbbaFleet.Features.$ARGUMENTS`
   - Class `$ARGUMENTSValidator : AbstractValidator<TRequest>` where `TRequest` is the identified type
   - A constructor with placeholder `RuleFor` calls for each property on the model (use `NotEmpty()` for required strings, `MaximumLength()` where appropriate)

Example shape:

```csharp
using FluentValidation;

namespace AbbaFleet.Features.Example;

public class ExampleValidator : AbstractValidator<UpsertExampleRequest>
{
    public ExampleValidator()
    {
        RuleFor(r => r.Name).NotEmpty().MaximumLength(100);
    }
}
```

3. After creating the file, remind the user:
   - **Always use `ValidateAsync` (not `Validate`)** when calling the validator. FluentValidation's async path supports async rules and is consistent — the synchronous `Validate` method will not run any async validators and can silently skip rules. Call it like:
     ```csharp
     var result = await _validator.ValidateAsync(request);
     if (!result.IsValid) { ... }
     ```
   - Wire the validator via DI if not already registered (e.g. `services.AddScoped<IValidator<UpsertExampleRequest>, ExampleValidator>()`).

4. Tell the user what file was created and what placeholder rules were added.
