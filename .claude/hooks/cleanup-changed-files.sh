#!/usr/bin/env bash
# Run cleanupcode once for all files changed during the last Claude response.
tmp=".claude/changed-files.tmp"
[ ! -s "$tmp" ] && exit 0

# Deduplicate and build semicolon-separated include mask
includes=$(sort -u "$tmp" | tr '\n' ';' | sed 's/;$//')
rm "$tmp"

dotnet jb cleanupcode \
  --profile="C# Cleanup" \
  --settings=abba-fleet.sln.DotSettings \
  abba-fleet.sln \
  --include="$includes" > /dev/null 2>&1
