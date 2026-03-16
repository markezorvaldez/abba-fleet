Create a pull request for the current branch.

1. Run `git log main..HEAD --oneline` to see what commits are included.
2. Identify the Linear ticket ID from the branch name (e.g. `feature/mar-13-login` → MAR-13).
3. Create the PR using `gh pr create` with:
   - Title: short description followed by the ticket ID in parentheses, e.g. `add login and logout (MAR-13)`
   - Base branch: `main`
   - Body using the pull request template format:

```
# Summary

Implements [MAR-XX](https://linear.app/mark-valdez/issue/MAR-XX)

## Changes

- <bullet points summarising what changed>

## Done when checklist

- [ ] Acceptance criteria from the Linear issue are met
- [ ] Tests written where applicable
- [ ] No debug code or commented-out code left in
```

4. Return the PR URL.
