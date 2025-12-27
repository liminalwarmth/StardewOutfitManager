# [Mod Name] — Agent Handbook

## Mission Snapshot

- SMAPI mod for Stardew Valley 1.6+ / SMAPI 4.x
- [1-2 sentences describing what this mod does]
- [Core technical challenge or pattern, e.g., "Harmony patching for X" or "Custom menu system"]
- See `Docs/SDV_Modding_Reference.md` for API patterns; `README.md` for player-facing docs

## Critical Rules

1. **Read the modding guide:** Before every task, read `Docs/SDV_Modding_Reference.md` to understand SDV 1.6 API patterns and common gotchas.

2. **Consult external docs:** For APIs not covered in the modding guide, check the [Stardew Valley Modding Wiki](https://stardewvalleywiki.com/Modding:Index) and [decompiled source](https://github.com/Dannode36/StardewValleyDecompiled).

3. **Build must pass:** Run `dotnet build` before reporting any task complete. Zero errors required.

4. **Multiplayer & gamepad support:** Consider both local split-screen and remote multiplayer for all changes. Ensure UI works with gamepad navigation.

5. **Journal your work:** Each branch gets a journal file in `Docs/journals/` named `YYYY-MM-DD_branch-name.md`. Add entries as you complete work.

6. **Read before writing:** Always read relevant source files completely before making changes. Don't assume—verify.

7. **Update docs:** When behavior changes, update relevant documentation and comments.

8. **Log deferred work:** Use `Docs/ROADMAP.md` for planned features and `Docs/BUGS.md` for known issues.

## Workflow

1. **Research:** Read `Docs/SDV_Modding_Reference.md`. Read recent journals in `Docs/journals/`. Review relevant source files. Plan your approach.
2. **Implement:** Follow SDV modding best practices and write high-quality, maintainable code.
3. **Validate:** Run `dotnet build` until zero errors.
4. **Document:** Add journal entry, update docs if behavior changed.
5. **Commit:** Stage, commit with clear message, push.

## Feature Overview

[List the mod's main features in bullet points. Include both player-facing features and any integration points (shop additions, NPC interactions, etc.). This helps orient work toward actual mod functionality.]

**Core Features:**
- [Primary feature]
- [Secondary feature]

**Integration:**
- [Shop integration, if any]
- [Event hooks, if any]

## Configuration

Config stored in `config.json`, editable via [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) or manually.

| Option | Default | Description |
|--------|---------|-------------|
| `ExampleOption` | `true` | [What this option controls] |

When adding new config options:
1. Add property to `ModConfig.cs`
2. Update GMCM registration (if using GMCM)
3. Document in this table

## Architecture Overview

```
/
├── [ModName].cs              # Entry point (Mod class)
├── ModConfig.cs              # Configuration model
├── manifest.json             # SMAPI metadata
│
├── /Managers                 # Core management systems
├── /Menus                    # IClickableMenu implementations
├── /Models                   # Data structures
├── /Utils                    # Helper methods
├── /Patches                  # Harmony patches (if used)
├── /Assets                   # Textures, data files
│
└── /Docs
    ├── SDV_Modding_Reference.md   # API quick reference
    ├── ROADMAP.md                 # Feature plans
    ├── BUGS.md                    # Known issues
    ├── /feature_specs             # Feature specifications
    └── /journals                  # Per-branch work logs
```

**Boundaries:**
- **Managers/** - State management, orchestration (may use SMAPI APIs)
- **Menus/** - UI code (IClickableMenu, drawing, input handling)
- **Models/** - Pure data structures, no game dependencies
- **Utils/** - Shared helpers (may be pure or use game APIs)
- **Patches/** - Harmony patches only; keep logic minimal, delegate to other modules

## Tools & Commands

| Command | Purpose |
|---------|---------|
| `dotnet build` | Compile the mod |
| SMAPI console | Check for runtime errors (in-game) |
| `list_items` | Look up item IDs (in-game console) |

## SDV/SMAPI Patterns

See `Docs/SDV_Modding_Reference.md` for comprehensive patterns including:
- Item identification (QualifiedItemId, ItemRegistry)
- State gating (Context.IsWorldReady, etc.)
- Content API (AssetRequested, ModContent vs GameContent)
- Data persistence (modData, SaveData, config)
- Multiplayer (PerScreen<T>, mutex locking)
- UI development (IClickableMenu patterns)
- Harmony patching best practices

## Documentation Map

| File | Purpose |
|------|---------|
| `README.md` | Player-facing docs |
| `CLAUDE.md` | Agent handbook (this file) |
| `Docs/SDV_Modding_Reference.md` | API patterns & gotchas |
| `Docs/ROADMAP.md` | Planned features |
| `Docs/BUGS.md` | Known issues |
| `Docs/journals/` | Per-branch work logs |
| `Docs/feature_specs/` | Feature specifications |

## References

- [Stardew Valley Modding Wiki](https://stardewvalleywiki.com/Modding:Index) - Comprehensive modding docs
- [SDV 1.6 Migration](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)
- [SMAPI 4.0 Migration](https://wiki.stardewvalley.net/Modding:Migrate_to_SMAPI_4.0)
- [Modding:Items](https://stardewvalleywiki.com/Modding:Items) - ItemRegistry docs
- [Decompiled Source](https://github.com/Dannode36/StardewValleyDecompiled)

**Reference Modders** (high-quality open source mods for best practices):
- [Pathoschild](https://github.com/Pathoschild) - SMAPI creator, gold standard patterns
- [spacechase0](https://github.com/spacechase0) - GMCM, SpaceCore, many utilities
- [atravita](https://github.com/atravita-mods) - Well-documented, modern patterns
