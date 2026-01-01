#!/bin/bash
# Pre-commit validation for SDV Outfit Manager
# Called by Claude Code hooks on Bash tool use
# Only runs validation for git commit commands

# Read the tool input from stdin (JSON)
INPUT=$(cat)

# Extract the command from JSON
COMMAND=$(echo "$INPUT" | grep -o '"command"[[:space:]]*:[[:space:]]*"[^"]*"' | sed 's/"command"[[:space:]]*:[[:space:]]*"//' | sed 's/"$//')

# Only run validation for git commit commands
if ! echo "$COMMAND" | grep -q "git commit"; then
    # Not a commit, allow it
    exit 0
fi

echo "🔍 Running pre-commit validation..."

# Change to project directory
cd "$CLAUDE_PROJECT_DIR"

# 1. Build check
echo ""
echo "📦 Checking build..."
BUILD_OUTPUT=$(dotnet build --verbosity quiet 2>&1)
BUILD_STATUS=$?

if [ $BUILD_STATUS -ne 0 ]; then
    echo "❌ Build failed! Fix errors before committing." >&2
    echo "$BUILD_OUTPUT" >&2
    exit 2  # Exit code 2 = block the tool call
fi
echo "✅ Build passed"

# 2. Check for journal entry (just warn, don't block)
BRANCH=$(git branch --show-current | sed 's/.*\///')
TODAY=$(date +%Y-%m-%d)
JOURNAL_PATTERN="Docs/journals/*${BRANCH}*.md"

if ! ls $JOURNAL_PATTERN 1>/dev/null 2>&1; then
    echo ""
    echo "⚠️  Warning: No journal found for branch '$BRANCH'"
    echo "   Consider creating: Docs/journals/${TODAY}_${BRANCH}.md"
fi

echo ""
echo "✅ Pre-commit validation passed"
exit 0
