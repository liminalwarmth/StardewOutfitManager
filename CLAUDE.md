# Stardew Outfit Manager - Claude Code Rules

## Project Description

**Stardew Outfit Manager** replaces Stardew Valley's default dresser/wardrobe ShopMenu with a comprehensive outfit management system.

### Key Features

- **Wardrobe Tab**: Visual clothing picker that lets players cycle through items in their dresser by slot (hat, shirt, pants, shoes, rings) with live preview on a rotating farmer portrait. Includes hair and accessory customization.

- **Favorites Tab**: Saved outfit system with seasonal categories (Spring, Summer, Fall, Winter, Special). Displays outfit cards showing a model farmer in each saved outfit, with availability indicators for missing items, last-worn timestamps, favorite pinning, and one-click equipping.

- **Dresser Tab**: Custom inventory view of dresser contents.

### How It Differs from Fashion Sense

This mod requires you to actually own the clothing items in your dresser. It doesn't spawn items or create virtual wardrobes - it manages and organizes what you have. Outfits gray out when pieces are missing from your dresser.

### Multiplayer Support

Each farmer has their own favorites data stored separately (keyed by player name, save folder, and unique multiplayer ID). Uses per-screen state management for local co-op and mutex locking to prevent dresser conflicts when multiple players try to access the same dresser.

### Technical Notes

- Uses Harmony patching for game customization
- Custom farmer rendering with proper layer management for previews
- Item tagging via modData to track outfit pieces even when items move around in the world

## Development Journal

**IMPORTANT:** Before starting any task, review `Docs/journal.md` to understand recent changes and context.

After completing any task, add a new entry to the TOP of the journal with:
- Date and timestamp (format: `## YYYY-MM-DD HH:MM - Task Title`)
- Brief bulleted summary of changes made
- Files modified
- Any issues encountered or decisions made

## Before Starting Any Task

1. **Research First**: Before making any code changes related to Stardew Valley or SMAPI APIs:
   - Check the [Stardew Valley 1.6 Migration Guide](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)
   - Check the [SMAPI 4.0 Migration Guide](https://wiki.stardewvalley.net/Modding:Migrate_to_SMAPI_4.0)
   - Search for specific API changes if unsure about method signatures or property names
   - Look at reference mods like [Fashion Sense](https://github.com/Floogen/FashionSense) for patterns

2. **Understand the Context**: Read the relevant source files completely before making changes. Don't assume - verify.

## Working From Plans

When following a plan document (like `Docs/SMAPI_1.6_Migration_Plan.md`):

1. **All plan items must have sub-items**: Break down each step into specific, actionable sub-tasks
2. **Check off items as completed**: Mark items with `[x]` as they are finished so progress is visible
3. **Track where you are**: Before resuming work, review the plan to see what's done and what's next

## Completion Requirements

Before reporting that any task is complete, you MUST:

1. **Verify All Steps Completed**: Review the task requirements and confirm each step was addressed
2. **Check for Missing Items**: Explicitly list what was changed and verify nothing was skipped
3. **Test Compilation**: Run `dotnet build` to verify the code compiles without errors
4. **Review TODOs**: Check if any new TODOs were introduced or if existing ones were addressed
5. **Update Documentation**: If behavior changed, update relevant docs or comments

## Project-Specific Notes

- This mod targets **Stardew Valley 1.6.x** with **SMAPI 4.x**
- Uses **Harmony** for patching game internals
- Key APIs: FarmerRenderer (custom drawing), StorageFurniture (dresser), IClickableMenu (UI)
- Deploy script: `./deploy.sh` builds and copies to Mods folder for testing

## Testing Workflow

1. Make changes
2. Run `./deploy.sh` to build and deploy
3. Launch Stardew Valley
4. Test at a dresser (farmhouse has one by default)
5. Check SMAPI console for errors

## Reference Resources

### Decompiled Game Source (for API research)
- **SDV 1.6 Decompiled**: [Dannode36/StardewValleyDecompiled](https://github.com/Dannode36/StardewValleyDecompiled)
  - ShopMenu.cs: `Stardew Valley/StardewValley.Menus/ShopMenu.cs`
  - FarmerRenderer.cs: `Stardew Valley/StardewValley/FarmerRenderer.cs`
- **SDV 1.5.6 Decompiled**: [WeDias/StardewValley](https://github.com/WeDias/StardewValley) - For comparing changes

### Migration Guides
- [SDV 1.6 Migration](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)
- [SMAPI 4.0 Migration](https://wiki.stardewvalley.net/Modding:Migrate_to_SMAPI_4.0)
- [SDV 1.6.16 Migration](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6.16) - Future version

### Reference Mods
- [Fashion Sense](https://github.com/Floogen/FashionSense) - FarmerRenderer patching patterns
