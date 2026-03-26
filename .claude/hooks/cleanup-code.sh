#!/usr/bin/env bash
# Extract file_path from the hook's JSON stdin, convert to a relative path,
# and run JetBrains cleanupcode on it.

# Read stdin into a variable
input=$(cat)

# Extract file_path value from JSON (strip everything before/after the value)
fp=$(echo "$input" | sed -n 's/.*"file_path":"\([^"]*\)".*/\1/p' | head -1)

[ -z "$fp" ] && exit 0

# JSON encodes backslashes as \\, convert to forward slashes
rel=$(echo "$fp" | sed 's|\\\\|/|g; s|.*/abba-fleet/||')

[ -z "$rel" ] && exit 0

dotnet jb cleanupcode --profile="C# Cleanup" abba-fleet.sln --include="$rel" > /dev/null 2>&1
