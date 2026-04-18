#!/usr/bin/env bash
# Append the changed file path to a temp list for batch processing at Stop.
input=$(cat)
fp=$(echo "$input" | sed -n 's/.*"file_path":"\([^"]*\)".*/\1/p' | head -1)
[ -z "$fp" ] && exit 0
rel=$(echo "$fp" | sed 's|\\\\|/|g; s|.*/abba-fleet/||')
[ -z "$rel" ] && exit 0
echo "$rel" >> .claude/changed-files.tmp
