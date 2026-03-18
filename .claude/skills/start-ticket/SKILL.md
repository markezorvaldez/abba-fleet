---
name: start-ticket
description: Start work on a Linear ticket. Use this when the user says "let's start [ticket]" or picks a ticket to work on.
---

Start work on a Linear ticket.

1. Fetch the full ticket details using `get_issue`.
2. **Check the assignee.** If the ticket is assigned to someone other than the user or is unassigned, stop and tell the user who is currently assigned so they can check with them before proceeding. Most likely it should be assigned to the user or nobody — but pause and let the user decide.
3. **Check the current status.** Only move the ticket to In Progress if it is currently in Todo or Backlog. Never move a ticket backward (e.g. do not change In Review → In Progress).
4. Set the ticket status to **In Progress** and assign it to the user (`me`).
5. Proceed with planning and implementation.
