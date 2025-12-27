# Stardew Outfit Manager — Agent Handbook

## Mission Snapshot

- SMAPI mod replacing the default dresser ShopMenu with a tabbed outfit management system
- Three tabs: Wardrobe (item picker), Favorites (saved outfits), Dresser (inventory)
- Core challenges: custom farmer rendering for previews, per-player data in multiplayer, item tracking via modData tags
- See `Docs/SDV_Modding_Reference.md` for API patterns; `README.md` for player-facing docs

## Critical Rules

1. **Read the modding guide:** Before every task, read `Docs/SDV_Modding_Reference.md` to understand SDV 1.6 API patterns and common gotchas.

2. **Consult external docs:** For APIs not covered in the modding guide, check the [Stardew Valley Modding Wiki](https://stardewvalleywiki.com/Modding:Index) and [decompiled source](https://github.com/Dannode36/StardewValleyDecompiled).

3. **Build must pass:** Run `dotnet build` before reporting any task complete. Zero errors required.

4. **Multiplayer & gamepad support:** Use `PerScreen<T>` for per-player state. Lock dressers via `mutex.RequestLock()`. Ensure all UI elements have proper gamepad navigation (myID, neighborIDs). Consider both local split-screen and remote multiplayer.

5. **Journal your work:** Each branch gets a journal file in `Docs/journals/` named `YYYY-MM-DD_branch-name.md`. Add entries as you complete work. Check recent journals before starting.

6. **Read before writing:** Always read relevant source files completely before making changes. Don't assume—verify.

7. **Log deferred work:** Use `Docs/ROADMAP.md` for planned features and `Docs/BUGS.md` for known issues.

## Workflow

1. **Research:** Read `Docs/SDV_Modding_Reference.md`. Read recent journals in `Docs/journals/`. Review relevant source files. Plan your approach.
2. **Implement:** Follow SDV modding best practices and write high-quality, maintainable code. Respect module boundaries.
3. **Validate:** Run `dotnet build` until zero errors.
4. **Document:** Add journal entry, update feature specs if needed.
5. **Commit:** Stage, commit with clear message, push.

## Feature Overview

**Core Features:**
- **Wardrobe Tab** - Visual item picker with live farmer preview; cycle through hats, shirts, pants, shoes, rings by slot
- **Favorites Tab** - Save outfits with seasonal categories (Spring/Summer/Fall/Winter/Special); shows availability status
- **Dresser Tab** - Inventory view of dresser contents with category filters

**Shop Integration:**
- New players start with a dresser in their farmhouse (configurable)
- Robin sells custom dressers (14 variants) at her shop
- Traveling Merchant occasionally stocks mirror-style dressers

**Multiplayer:**
- Per-player favorites data (keyed by player name + save folder + multiplayer ID)
- Mutex locking prevents dresser conflicts between players

## Configuration

Config stored in `config.json`, editable via [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) or manually.

| Option | Default | Description |
|--------|---------|-------------|
| `StartingDresser` | `true` | Give new players a dresser in their farmhouse |
| `RobinSellsDressers` | `true` | Add custom dressers to Robin's shop inventory |
| `TravelingMerchantSellsDressers` | `true` | Add mirror dressers to Traveling Merchant stock |

When adding new config options, update both `ModConfig.cs` and the GMCM registration in `StardewOutfitManager.cs`.

## Architecture Overview

```
/
├── StardewOutfitManager.cs   # Entry point, event handling, menu interception
├── ModConfig.cs              # Config (StartingDresser, RobinSellsDressers, TravelingMerchant)
├── manifest.json             # SMAPI metadata
│
├── /Managers
│   ├── MenuManager.cs        # Tab system, top bar UI, shared state across tabs
│   ├── PlayerManager.cs      # Per-screen state, favorites loading/saving
│   └── AssetManager.cs       # Custom dresser content, shop integration
│
├── /Menus
│   ├── WardrobeMenu.cs       # Item picker with live farmer preview
│   ├── FavoritesMenu.cs      # Saved outfits grid, equipping, naming
│   └── NewDresserMenu.cs     # Dresser inventory (ShopMenu port)
│
├── /Models
│   └── FavoritesModel.cs     # FavoritesData, FavoriteOutfit structures
│
├── /Utils
│   ├── OutfitMethods.cs      # Equipment swapping between farmer/dresser
│   ├── FavoritesMethods.cs   # Outfit save/load, item tagging, availability
│   └── ModTools.cs           # Custom farmer rendering (drawFarmerScaled)
│
├── /Assets
│   ├── /Objects/Furniture/   # Custom dresser sprites (14 variants)
│   └── /UI/                  # Custom sprite sheet
│
└── /Docs
    ├── SDV_Modding_Reference.md   # API quick reference
    ├── ROADMAP.md                 # Feature plans
    ├── BUGS.md                    # Known issues
    ├── /feature_specs             # Feature specifications
    └── /journals                  # Per-branch work logs
```

**Module Boundaries:**
- **StardewOutfitManager.cs** - Entry point only. Registers events, creates managers, intercepts dresser menu.
- **Managers/** - Orchestration layer. MenuManager owns tab state. PlayerManager owns per-player data. AssetManager handles content API.
- **Menus/** - UI code. Each menu is self-contained. All use MenuManager for tab bar.
- **Models/** - Pure data structures. No game API dependencies.
- **Utils/** - Helpers. OutfitMethods = equip logic. FavoritesMethods = data ops. ModTools = custom rendering.

## Tools & Commands

| Command | Purpose |
|---------|---------|
| `dotnet build` | Compile the mod |
| SMAPI console | Check for runtime errors (in-game) |
| `list_items` | Look up item IDs (in-game console) |

## Key Patterns in This Codebase

### Menu Interception
```csharp
// In OnMenuRender: detect dresser ShopMenu and replace
if (Game1.activeClickableMenu is ShopMenu shop && shop.source == dresser)
    dresser.mutex.RequestLock(delegate { /* open custom menu */ });
```

### Tab System
```csharp
// Each menu calls this in constructor to add tab buttons
menuManager.includeTopTabButtons(this);
```

### Item Tagging
```csharp
// Tag items for tracking
item.modData["StardewOutfitManagerFavoriteItem"] = $"{item.Name}_{Guid.NewGuid()}";
// Look up by tag
var lookup = FavoritesMethods.BuildItemTagLookup(dresser);
```

### Per-Player Data Storage
```csharp
// Path: data/favoritesData/{PlayerName}_{SaveFolder}_{MultiplayerID}.json
Helper.Data.WriteJsonFile<FavoritesData>(path, data);
```

### Custom Farmer Rendering
```csharp
// In ModTools.cs - renders farmer at 4x scale for previews
DrawCustom.drawFarmerScaled(b, farmer, position, facingDirection);
```

## Documentation Map

| File | Purpose |
|------|---------|
| `README.md` | Player-facing overview |
| `CLAUDE.md` | Agent handbook (this file) |
| `Docs/SDV_Modding_Reference.md` | API patterns & 1.6 gotchas |
| `Docs/ROADMAP.md` | Planned features by priority |
| `Docs/BUGS.md` | Known issues |
| `Docs/feature_specs/` | Feature specifications |
| `Docs/journals/` | Per-branch work logs |
| `Docs/journals/_archive.md` | Historical entries pre-branch-journaling |

## References

- [Stardew Valley Modding Wiki](https://stardewvalleywiki.com/Modding:Index) - Comprehensive modding docs
- [SDV 1.6 Migration](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)
- [SMAPI 4.0 Migration](https://wiki.stardewvalley.net/Modding:Migrate_to_SMAPI_4.0)
- [Modding:Items](https://stardewvalleywiki.com/Modding:Items)
- [Decompiled Source](https://github.com/Dannode36/StardewValleyDecompiled)

**Reference Modders** (high-quality open source mods for best practices):
- [Pathoschild](https://github.com/Pathoschild) - SMAPI creator, gold standard patterns
- [spacechase0](https://github.com/spacechase0) - GMCM, SpaceCore, many utilities
- [atravita](https://github.com/atravita-mods) - Well-documented, modern patterns
