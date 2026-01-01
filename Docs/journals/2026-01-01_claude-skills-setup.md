# Claude Skills Setup

**Branch:** liminalwarmth/charlotte
**Date:** 2026-01-01
**Status:** Complete

## Summary

Implemented Claude Code skills, agents, and hooks to improve development efficiency for this SDV modding project. The goal is autonomous invocation by Claude during development work.

## Changes Made

### CLAUDE.md Updates
- Added Roles section defining user as product director/designer and Claude as full implementer
- Added Skills to Invoke section with table of skills and when to use them
- Updated Workflow section to reference skills and pre-commit validation

### Skills Created

1. **UI Layout Designer** (`.claude/skills/sdv-ui-layout/`)
   - Conceptual design workflow with ASCII diagrams
   - Semantic positioning patterns
   - Layout constants reference
   - Static analysis checklist
   - Debug overlay guidance

2. **Multiplayer/Gamepad Audit** (`.claude/skills/sdv-mp-gamepad-audit/`)
   - PerScreen wrapper validation
   - Context check verification
   - Mutex locking patterns
   - Gamepad navigation ID requirements
   - Split-screen considerations

3. **Data Validation** (`.claude/skills/sdv-data-validation/`)
   - FavoritesData schema validation
   - ModConfig validation
   - ModData key conventions
   - Save file path patterns
   - Migration guidance for breaking changes

4. **Session Orientation** (`.claude/skills/sdv-session-start/`)
   - Quick ramp-up at session start
   - Journal reading workflow
   - Git status checking
   - Build verification

### Hook Created

5. **Pre-Commit Validation** (`.claude/hooks/hooks.json`)
   - PreToolUse hook on git commit
   - Validates build, multiplayer/gamepad, data structures
   - Blocks commit until issues resolved

### .gitignore Updates
- Added `.reference/` for cloned reference mods
- Added `.context/` for Conductor workspace files

## Files Modified
- `CLAUDE.md` - Added roles, skills table, updated workflow
- `.gitignore` - Added reference and context directories

## Files Created
- `.claude/skills/sdv-ui-layout/SKILL.md`
- `.claude/skills/sdv-ui-layout/references/codebase-patterns.md`
- `.claude/skills/sdv-ui-layout/examples/layout-example.md`
- `.claude/skills/sdv-mp-gamepad-audit/SKILL.md`
- `.claude/skills/sdv-mp-gamepad-audit/references/audit-checklist.md`
- `.claude/skills/sdv-data-validation/SKILL.md`
- `.claude/skills/sdv-data-validation/references/data-structures.md`
- `.claude/skills/sdv-session-start/SKILL.md`
- `.claude/hooks/hooks.json`

## Build Status
Build succeeded with 0 warnings, 0 errors.

## Validation & Fixes

After initial implementation, researched Claude Code skill documentation to validate format:

**Issues Found & Fixed:**
1. Skill descriptions used second-person ("Use when...") instead of third-person ("This skill should be used when...")
   - Fixed all 4 skills and 1 agent to use correct format
2. hooks.json used non-standard format with object matchers
   - Fixed to use standard Claude Code format with string matchers
3. Description pattern didn't include example trigger phrases
   - Added quoted phrases like "create a menu layout", "fix UI spacing"

**Validated as Correct:**
- Directory structure (.claude/skills/skill-name/SKILL.md)
- Frontmatter format (name, description, version)
- Agent format with model field
- Progressive disclosure with references/ and examples/ directories

## Refinements (Same Day)

After testing skills, made the following refinements:

1. **Made Session Orientation mandatory (Rule 0)**
   - Added to CLAUDE.md Critical Rules as rule 0
   - Updated skill description to use "ALWAYS", "FIRST", "Do NOT skip"
   - Version bumped to 1.1.0

2. **Removed Reference Mod Research agent**
   - Web search and wiki are faster alternatives
   - Kept reference modders list in CLAUDE.md for manual lookup

3. **Fixed modData key example in CLAUDE.md**
   - Changed from unprefixed legacy key to proper `LiminalWarmth.StardewOutfitManager/` prefix

## Hook Fix (PR Review Follow-up)

During PR review, discovered the pre-commit hook wasn't firing:

**Root Cause:**
- Hook config was in `.claude/hooks/hooks.json` (wrong location)
- Should be in `.claude/settings.json` under a `hooks` key
- The `type: "prompt"` was advisory only; needed `type: "command"` for actual blocking

**Fix Applied:**
1. Created `.claude/settings.json` with proper hook configuration
2. Updated `pre-commit-check.sh` to:
   - Read JSON from stdin to get the Bash command
   - Only run validation when command contains "git commit"
   - Use exit code 2 to block commits on build failure
3. Removed obsolete `.claude/hooks/hooks.json`

**Note:** Hooks may not fire in Conductor environment due to different tool invocation path. The configuration is now correct for native Claude Code usage.

## Next Steps
- Continue testing skills in development work
- Add missing reference materials as needed
