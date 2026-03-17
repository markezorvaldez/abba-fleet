---
name: create-pr
description: Create a pull request for the current branch linked to the Linear ticket. Use this when the user asks to create or raise a PR. Do not use if the user explicitly says not to use the skill.
---

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

<1-3 sentence behaviour-level summary — what the user can now do, not what files changed.
Reviewers can see the diff; they need context, not a file list.>

## Notes

<Only include if there are non-obvious decisions worth calling out — e.g. why a particular
approach was chosen, anything that will surprise a reviewer. Omit this section if there
is nothing worth noting.>

## Done when checklist

- [ ] Acceptance criteria from the Linear issue are met
- [ ] Tests written where applicable
- [ ] No debug code or commented-out code left in

## Screenshots

<!-- Add screenshots showing the feature working locally. Delete this section if not applicable. -->
```

   Append this footnote after the template body (not part of the template itself):

   ```
   ---
   🤖 Generated with [Claude Code](https://claude.ai/claude-code)
   ```

4. Return the PR URL.
